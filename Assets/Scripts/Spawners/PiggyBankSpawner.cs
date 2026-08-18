using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PiggyBankSpawner : NetworkBehaviour {
    public static PiggyBankSpawner instance;
    public GameObject piggyBankPrefab;
    public GameObject piggyBankFracturedPrefab;
    public GameObject coinPrefab;
    public Vector3 spawnPos;

    void Awake() {
        instance = this;
        spawnPos = new Vector3(0.18f, 0.18f, 1);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Y)) {
            SpawnPiggyBankRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPiggyBankRpc() {
        spawnPos = Player.localPlayer.transform.position + new Vector3(0, 0.5f, 0);
        GameObject piggyBank = Instantiate(piggyBankPrefab, spawnPos, Quaternion.identity);
        NetworkObject networkObject = piggyBank.GetComponent<NetworkObject>();
        networkObject.Spawn();
        GameManager.Instance.spawnedObjectsToDespawn.Add(networkObject);
    }

}
