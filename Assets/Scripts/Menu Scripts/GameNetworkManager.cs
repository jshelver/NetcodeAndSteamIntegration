using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    public static GameNetworkManager instance;

    public static Lobby? currentLobby = null;

    FacepunchTransport transport;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton != null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public async void StartLobbyHost(int maxClients = 4)
    {
        // Create a new steam lobby
        await SteamMatchmaking.CreateLobbyAsync(maxClients);

        transport.targetSteamId = SteamClient.SteamId;

        if (NetworkManager.Singleton.StartHost())
            Debug.Log("Host Started");
        else
            Debug.Log("Host Failed to Start");
    }

    public void StartLobbyClient(Lobby lobby)
    {
        transport.targetSteamId = lobby.Owner.Id;

        UIManager.instance.StartClient(lobby);

        if (NetworkManager.Singleton.StartClient())
            Debug.Log("Client Started");
        else
            Debug.Log("Client Failed to Start");
    }

    public void Disconnect()
    {
        currentLobby?.Leave();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.Shutdown();
    }

    #region Steam Callbacks

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError($"Failed to create lobby: {result}", this);
            UIManager.instance.LobbyCreationFailed();
            return;
        }

        string lobbyName = lobby.Owner.Name + "'s Lobby";

        lobby.SetFriendsOnly();
        lobby.SetData("HostAddress", lobbyName);
        lobby.SetJoinable(true);

        UIManager.instance.UpdateLobbyMenu(lobby);

        currentLobby = lobby;
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost) return;

        Debug.Log($"Lobby Successfully Joined: {lobby.Id}", this);
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} Joined", this);
        UIManager.instance.UpdateLobbyMenu(lobby);
    }

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} Left", this);
        UIManager.instance.UpdateLobbyMenu(lobby);
    }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        // Try to enter lobby
        RoomEnter tryJoinLobbySuccess = await lobby.Join();

        if (tryJoinLobbySuccess != RoomEnter.Success)
        {
            Debug.LogError($"Failed to Join Lobby: {tryJoinLobbySuccess}", this);
            return;
        }

        // Once client has checked if it has permission to enter lobby, actually connect to steam lobby
        await SteamMatchmaking.JoinLobbyAsync(lobby.Id);

        currentLobby = lobby;

        // Now connect client to server (via steam socket)
        StartLobbyClient(lobby);
    }

    #endregion

    #region Network Callbacks

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected: {clientId}", this);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client Disconnected: {clientId}", this);
    }

    #endregion

    #region Scene Management

    [ServerRpc]
    public void StartGameServerRpc()
    {
        if (!currentLobby.HasValue) return;

        currentLobby.Value.SetJoinable(false);

        UIManager.instance.gameObject.GetComponent<NetworkObject>().Despawn();

        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        // Updates scene to the game scene
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(1);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        SpawnPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Spawn the player on the server and get the NetworkObject
        GameObject player = Instantiate(PlayerData.instance.playerPrefab, Vector3.up, Quaternion.identity);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();

        // Spawn the player on the clients with correct owner
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        networkObject.SpawnWithOwnership(clientId);
    }

    #endregion
}
