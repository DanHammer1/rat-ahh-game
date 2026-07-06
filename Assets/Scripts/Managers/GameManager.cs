using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Collections;
using Unity.Collections;

public class GameManager : NetworkBehaviour
{
    public enum PlayerRole {
        Hunter,
        Hider
    };

    public static GameManager Instance;
    public static bool playersSpawned = false;

    public GameObject ratPrefab;
    public GameObject hunterPrefab;
    public bool sceneReady = false;

    public NetworkList<ulong> clientIds = new NetworkList<ulong>();
    public NetworkList<FixedString32Bytes> clientNames = new NetworkList<FixedString32Bytes>();
    public NetworkList<int> clientRoles = new NetworkList<int>();

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private static List<ulong> GetIds(int role) {
        List<ulong> newList = new List<ulong>();

        for (int i = 0; i < Instance.clientRoles.Count; i++) {
            if (Instance.clientRoles[i] == role) newList.Add(Instance.clientIds[i]);
        }

        return newList;
    }

    private static List<int> GetIndexs(int role) {
        List<int> newList = new List<int>();

        for (int i = 0; i < Instance.clientRoles.Count; i++) {
            if (Instance.clientRoles[i] == role) newList.Add(i);
        }

        return newList;
    }

    public static List<ulong> GetHunterIds() {
        return GetIds((int)PlayerRole.Hunter);
    }

    public static List<ulong> GetHiderIds() {
        return GetIds((int)PlayerRole.Hider);
    }

    public static List<int> GetHunterIndexs() {
        return GetIndexs((int)PlayerRole.Hunter);
    }

    public static List<int> GetHiderIndexs() {
        return GetIndexs((int)PlayerRole.Hider);
    }

    public void AssignPlayerRoles() {
        for (int i = 0; i < clientIds.Count; i++) {
            PlayerRole randRole = GetRandomEnumType<PlayerRole>();
            if (clientRoles[i] == -1) clientRoles[i] = (int)randRole;
        }
    }

    public static T GetRandomEnumType<T>() {
        System.Array values = System.Enum.GetValues(typeof(T));
        int index = UnityEngine.Random.Range(0, values.Length);
        return (T)values.GetValue(index);
    }

    public static PlayerRole GetRole(ulong clientId) {
        return (PlayerRole)Instance.clientRoles[Instance.clientIds.IndexOf(clientId)];
    }

    void SpawnPlayer(GameManager.PlayerRole role, ulong clientId) {
        if (!IsServer) return;

        GameObject playerInstance;
        if (role == GameManager.PlayerRole.Hunter) {
            playerInstance = Instantiate(hunterPrefab);
        }
        else {
            playerInstance = Instantiate(ratPrefab);
        }
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
        
        netObj.SpawnAsPlayerObject(clientId, true);
        netObj.GetComponent<Player>().clientId.Value = clientId;
        
        //targetGroup.AddMember(playerInstance.transform.GetChild(1), 1f, 5f);
    }

    public IEnumerator SpawnAllPlayers() {
        if (!IsServer || playersSpawned) yield break;
        
        while (!sceneReady) {
            yield return null;
        }

        AssignPlayerRoles();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(GetRole(clientId), clientId);
        }

        playersSpawned = true;
    }

    [ClientRpc]
    public void OnGameStartClientRpc() {
        gameObject.GetComponent<ProgressManager>().enabled = true;
        StartCoroutine(gameObject.GetComponent<ProgressManager>().OnActivate());
    }

    public static ulong GetLocalId() {
        return NetworkManager.Singleton.LocalClientId;
    }

    public static PlayerRole GetLocalRole() {
        return (PlayerRole)Instance.clientRoles[Instance.clientIds.IndexOf(GetLocalId())];
    }

    public static FixedString32Bytes GetLocalName() {
        return Instance.clientNames[Instance.clientIds.IndexOf(GetLocalId())];
    }
}