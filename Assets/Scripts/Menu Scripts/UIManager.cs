using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UIManager : NetworkBehaviour
{
    public static UIManager instance;

    [Header("References")]
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject lobbyMenu;
    [SerializeField] TextMeshProUGUI lobbyTitleText;
    [SerializeField] GameObject playerListItem;
    [SerializeField] GameObject playerListHolder;
    [SerializeField] Button startButton;
    [SerializeField] GameObject playerPrefab;

    void Start()
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
        lobbyMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void StartHost()
    {
        mainMenu.SetActive(false);

        int maxClients = 4;
        GameNetworkManager.instance.StartLobbyHost(maxClients);
    }

    public void StartClient(Lobby lobby)
    {
        mainMenu.SetActive(false);
        UpdateLobbyMenu(lobby);
    }

    public void UpdateLobbyMenu(Lobby lobby)
    {
        lobbyMenu.SetActive(true);
        SetLobbyTitle(lobby);
        UpdateMemberList(lobby.Members);
        if (lobby.IsOwnedBy(SteamClient.SteamId))
        {
            // Only the lobby owner can start the game
            startButton.interactable = true;
        }
        else
        {
            startButton.interactable = false;
        }
    }

    private void SetLobbyTitle(Lobby lobby)
    {
        string lobbyName = lobby.Owner.Name + "'s Lobby";

        lobbyTitleText.text = string.IsNullOrEmpty(lobbyName) ? "Lobby" : lobbyName;
    }

    public void LobbyCreationFailed()
    {
        mainMenu.SetActive(true);
    }

    public void LeaveLobby()
    {
        GameNetworkManager.instance.Disconnect();
        lobbyMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    private void UpdateMemberList(IEnumerable<Friend> members)
    {
        foreach (Transform child in playerListHolder.transform)
        {
            // Destroy all old player list items
            if (child.gameObject.GetComponent<PlayerListItemData>() != null)
            {
                Destroy(child.gameObject);
            }
        }

        // Create new player list items for all members
        foreach (Friend member in members)
        {
            AddPlayerToList(member.Name);
        }
    }

    private void AddPlayerToList(string playerName)
    {
        PlayerListItemData newPlayer = Instantiate(playerListItem, playerListHolder.transform).GetComponent<PlayerListItemData>();
        newPlayer.transform.SetAsLastSibling();
        newPlayer.playerNameText.text = playerName;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void InvitePlayers()
    {
        SteamFriends.OpenGameInviteOverlay(GameNetworkManager.currentLobby.Value.Id);
    }

    public void StartGame()
    {
        GameNetworkManager.instance.StartGameServerRpc();
    }
}
