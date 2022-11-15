using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameNetworkManager))]
public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public GameObject playerPrefab;

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
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {

    }
}
