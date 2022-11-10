using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject playerPrefab;

    void Start()
    {
        GameObject player = Instantiate(playerPrefab, Vector3.up, Quaternion.identity);
        NetworkObject networkObject = player.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
    }

    void Update()
    {

    }
}
