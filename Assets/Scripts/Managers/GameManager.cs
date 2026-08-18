using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Collections;
using Unity.Collections;
using UnityEngine.SceneManagement;
using FMODUnity;
public class GameManager : NetworkBehaviour {
    public enum PlayerRole {
        HUNTER,
        HIDER
    };
    public enum GameState {
        MAINMENU,
        LOBBY,
        GAME,
    }

    public static GameManager Instance;
    public static bool playersSpawned = false;
    public static GameState gameState = GameState.MAINMENU;

    public GameObject ratPrefab;
    public GameObject hunterPrefab;
    public bool sceneReady = false;

    public NetworkList<ulong> clientIds = new NetworkList<ulong>();
    public NetworkList<FixedString32Bytes> clientNames = new NetworkList<FixedString32Bytes>();
    public NetworkList<int> clientRoles = new NetworkList<int>();

    public List<NetworkObject> spawnedObjectsToDespawn = new List<NetworkObject>();

    void Awake() {
        if (Instance != null && Instance != this) {
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

    public void DespawnObjects() {
        foreach (NetworkObject objectToDespawn in spawnedObjectsToDespawn) {
            objectToDespawn.Despawn(true);
        }
        spawnedObjectsToDespawn.Clear();
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

    public void SpawnPlayer(GameManager.PlayerRole role, ulong clientId, Vector3 spawnPos = default, Quaternion spawnRotation = default) {
        if (!IsServer) return;

        GameObject playerInstance;
        if (role == GameManager.PlayerRole.HUNTER) {
            playerInstance = Instantiate(hunterPrefab, spawnPos, spawnRotation);
        } else {
            playerInstance = Instantiate(ratPrefab, spawnPos, spawnRotation);
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
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            SpawnPlayer(GetRole(clientId), clientId);
        }

        playersSpawned = true;
    }

    [ClientRpc]
    public void OnGameStartClientRpc() {
        gameObject.GetComponent<ProgressManager>().enabled = true;
        StartCoroutine(gameObject.GetComponent<ProgressManager>().OnActivate());
    }
    [ClientRpc]
    public void OnLobbyStartClientRpc() {
        gameObject.GetComponent<ProgressManager>().OnDeactivate();
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

    public static void PlayLocalSoundEffectInWorld(Assets.SfxType soundEffect, Vector3 worldPosition) {
        RuntimeManager.PlayOneShot(Assets.instance.GetEventReferenceFromSfxType(soundEffect), worldPosition);
    }

    public static void PlayLocalSoundEffectInWorld(Assets.SfxType soundEffect) {
        RuntimeManager.PlayOneShot(Assets.instance.GetEventReferenceFromSfxType(soundEffect), Player.localPlayer.transform.position);
    }

    public static void PlayGlobalSoundEffectInWorld(Assets.SfxType soundEffect, Vector3 worldPosition) {
        GameManager.Instance.PlayGlobalSoundEffectInWorldClientRpc(soundEffect, worldPosition);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    private void PlayGlobalSoundEffectInWorldClientRpc(Assets.SfxType soundEffect, Vector3 worldPosition) {
        PlayLocalSoundEffectInWorld(soundEffect, worldPosition);
    }

    public static void PlayGlobalSoundEffectInWorld(Assets.SfxType soundEffect) {
        GameManager.Instance.PlayGlobalSoundEffectInWorldClientRpc(soundEffect, Player.localPlayer.transform.position);
    }
}