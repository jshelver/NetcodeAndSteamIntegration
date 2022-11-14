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
        if (NetworkManager.Singleton.StartHost())
        {
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            networkObject.Spawn();
        }
    }

    void Update()
    {

    }

}
