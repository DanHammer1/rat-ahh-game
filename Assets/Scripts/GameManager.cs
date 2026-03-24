using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
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
    public static int updateCount;

    public GameObject ratPrefab;
    public GameObject hunterPrefab;

    public NetworkList<ulong> clientIds = new NetworkList<ulong>();
    public NetworkList<FixedString32Bytes> clientNames = new NetworkList<FixedString32Bytes>();
    public NetworkList<int> clientRoles = new NetworkList<int>();

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (clientIds != null)
            clientIds.OnListChanged -= ClientIdsChanged;

        if (clientNames != null)
            clientNames.OnListChanged -= ClientNamesChanged;

        if (clientRoles != null)
            clientRoles.OnListChanged -= ClientRolesChanged;

        clientIds.OnListChanged += ClientIdsChanged;

        clientNames.OnListChanged += ClientNamesChanged;

        clientRoles.OnListChanged += ClientRolesChanged;
    }

    private void ClientIdsChanged(NetworkListEvent<ulong> changeEvent)
    {
        updateCount++;
    }

    private void ClientNamesChanged(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        updateCount++;
    }

    private void ClientRolesChanged(NetworkListEvent<int> changeEvent) {
        updateCount++;
    }

    public void AssignPlayerRoles() {
        for (int i = 0; i < clientIds.Count; i++) {
            PlayerRole randRole = GetRandomEnumType<PlayerRole>();
            if (clientRoles[i] == -1) clientRoles[i] = (int)randRole;
        }
    }

    public static T GetRandomEnumType<T>() {
        System.Array values = System.Enum.GetValues(typeof(T));
        int index = Random.Range(0, values.Length);
        return (T)values.GetValue(index);
    }

    public PlayerRole GetRole(ulong clientId) {
        return (PlayerRole)clientRoles[clientIds.IndexOf(clientId)];
    }

    void SpawnPlayer(GameManager.PlayerRole role, ulong clientId) {
        GameObject playerInstance;
        if (role == GameManager.PlayerRole.Hunter) {
            playerInstance = Instantiate(hunterPrefab);
        }
        else {
            playerInstance = Instantiate(ratPrefab);
        }
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
        //targetGroup.AddMember(playerInstance.transform.GetChild(1), 1f, 5f);
    }

    public void SpawnAllPlayers() {
        if (!IsServer || playersSpawned) return;

        AssignPlayerRoles();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(GetRole(clientId), clientId);
        }

        playersSpawned = true;
    }
}
