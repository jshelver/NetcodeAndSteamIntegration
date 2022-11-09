using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("References")]
    [SerializeField] GameObject startHostButton;
    [SerializeField] GameObject lobbyMenu;
    [SerializeField] TextMeshProUGUI lobbyTitleText;
    [SerializeField] GameObject playerListItem;
    [SerializeField] GameObject playerListHolder;

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
        startHostButton.SetActive(true);
    }

    void Update()
    {

    }

    public void StartHost(int maxClients)
    {
        startHostButton.SetActive(false);
        GameNetworkManager.instance.StartHost(maxClients);
    }

    public void StartClient(Lobby lobby)
    {
        startHostButton.SetActive(false);
        UpdateLobbyMenu(lobby);
    }

    public void UpdateLobbyMenu(Lobby lobby)
    {
        lobbyMenu.SetActive(true);
        SetLobbyTitle(lobby);
        UpdateMemberList(lobby.Members);
    }

    public void SetLobbyTitle(Lobby lobby)
    {
        string lobbyName = lobby.Owner.Name + "'s Lobby";

        lobbyTitleText.text = string.IsNullOrEmpty(lobbyName) ? "Lobby" : lobbyName;
    }

    public void LobbyCreationFailed()
    {
        startHostButton.SetActive(true);
    }

    public void LeaveLobby()
    {
        GameNetworkManager.instance.Disconnect();
        lobbyMenu.SetActive(false);
        startHostButton.SetActive(true);
    }

    public void UpdateMemberList(IEnumerable<Friend> members)
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
}
