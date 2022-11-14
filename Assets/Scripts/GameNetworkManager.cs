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
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public async void StartLobbyHost(int maxClients = 100)
    {
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
        lobby.SetData("name", lobbyName);
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
        RoomEnter joinedLobbySuccess = await lobby.Join();

        await SteamMatchmaking.JoinLobbyAsync(lobby.Id);

        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Debug.LogError($"Failed to Join Lobby: {joinedLobbySuccess}", this);
            return;
        }

        currentLobby = lobby;

        StartLobbyClient(lobby);
    }

    #endregion
}
