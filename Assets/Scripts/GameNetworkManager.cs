using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameNetworkManager : MonoBehaviour
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
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public async void StartHost(int maxClients = 100)
    {
        // NetworkManager.Singleton.OnServerStarted += OnServerStarted;

        // if (NetworkManager.Singleton.StartHost())
        //     Debug.Log("Host Started");
        // else
        //     Debug.Log("Host Failed to Start");

        await SteamMatchmaking.CreateLobbyAsync(maxClients);
    }

    public void StartClient(SteamId steamId, Lobby lobby)
    {
        // NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        // NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        string lobbyName = lobby.GetData("name");

        UIManager.instance.StartClient(lobby);

        transport.targetSteamId = steamId;

        // if (NetworkManager.Singleton.StartClient())
        //     Debug.Log("Client Started");
        // else
        //     Debug.Log("Client Failed to Start");
    }

    public void Disconnect()
    {
        currentLobby?.Leave();

        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.Shutdown();
    }

    public void StartGame()
    {
        if (!currentLobby.HasValue) return;

        SceneManager.LoadScene(1);
        if (currentLobby.Value.IsOwnedBy(SteamClient.SteamId))
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;

            if (NetworkManager.Singleton.StartHost())
                Debug.Log("Host Started");
            else
                Debug.Log("Host Failed to Start");
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

            if (NetworkManager.Singleton.StartClient())
                Debug.Log("Client Started");
            else
                Debug.Log("Client Failed to Start");
        }
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
        lobby.SetData("name", lobbyName);
        lobby.SetJoinable(true);

        UIManager.instance.UpdateLobbyMenu(lobby);

        currentLobby = lobby;
        Debug.Log($"Lobby Created: {lobby.Id}", this);
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

    private void OnLobbyInvite(Friend friend, Lobby lobby) => Debug.Log($"You got a invite from {friend.Name}", this);

    private void OnLobbyGameCreated(Lobby lobby, uint value, ushort value2, SteamId steamId) { }

    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        Debug.Log($"Joining Lobby: {lobby.Id}", this);
        RoomEnter joinedLobbySuccess = await lobby.Join();

        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Debug.LogError($"Failed to Join Lobby: {joinedLobbySuccess}", this);
            return;
        }
        else
        {
            currentLobby = lobby;
        }

        StartClient(lobby.Id, lobby);
    }

    #endregion

    #region Network Callbacks

    private void OnServerStarted() => Debug.Log("Server Started", this);

    private void OnClientConnected(ulong clientId) => Debug.Log($"Client Connected, clientId: {clientId}");

    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"Client Disconnected, clientId: {clientId}");

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    #endregion

}
