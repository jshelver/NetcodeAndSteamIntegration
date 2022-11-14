using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] GameObject playerPrefab;

    void Start()
    {
        SpawnPlayerServerRpc();
    }

    void Update()
    {

    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        // Spawn the player on the server and get the NetworkObject
        GameObject player = Instantiate(playerPrefab, Vector3.up, Quaternion.identity);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();

        // Spawn the player on the clients with correct owner
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        networkObject.SpawnWithOwnership(clientId);
    }

}
