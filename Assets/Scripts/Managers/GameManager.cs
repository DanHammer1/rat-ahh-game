using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Collections;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public enum PlayerRole {
        HUNTER,
        HIDER
    };

    public static GameManager Instance;
    public static bool playersSpawned = false;

    public static Action onExit;

    public GameObject ratPrefab;
    public GameObject hunterPrefab;
    public bool sceneReady = false;

    public NetworkList<ulong> clientIds = new NetworkList<ulong>();
    public NetworkList<FixedString32Bytes> clientNames = new NetworkList<FixedString32Bytes>();
    public NetworkList<int> clientRoles = new NetworkList<int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += (scene, b) => {
            sceneReady = false;
            playersSpawned = false;

            if (scene.name == "MainMenu") {
                GetComponent<ProgressManager>().IsActive = false;
                GetComponent<ProgressManager>().onActivateExecuted = false;
            }
        };
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
        return GetIds((int)PlayerRole.HUNTER);
    }

    public static List<ulong> GetHiderIds() {
        return GetIds((int)PlayerRole.HIDER);
    }

    public static List<int> GetHunterIndexs() {
        return GetIndexs((int)PlayerRole.HUNTER);
    }

    public static List<int> GetHiderIndexs() {
        return GetIndexs((int)PlayerRole.HIDER);
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
        if (role == GameManager.PlayerRole.HUNTER) {
            playerInstance = Instantiate(hunterPrefab);
        }
        else {
            playerInstance = Instantiate(ratPrefab);
        }
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
        
        netObj.SpawnAsPlayerObject(clientId, true);
        netObj.GetComponent<Player>().clientId.Value = clientId;
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

    public static void ExitToMainMenu() {
        onExit?.Invoke();
    }
}