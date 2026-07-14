using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PiggyBankSpawner : NetworkBehaviour
{
    public static PiggyBankSpawner instance;
    public GameObject piggyBankPrefab;
    public GameObject piggyBankFracturedPrefab;
    public GameObject coinPrefab;
    public Vector3 spawnPos;

    void Awake()
    {
        instance = this;
        spawnPos = new Vector3(0.18f, 0.68f, 1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SpawnPiggyBankRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPiggyBankRpc()
    {
        GameObject piggyBank = Instantiate(piggyBankPrefab, spawnPos, Quaternion.identity);
        piggyBank.GetComponent<NetworkObject>().Spawn();
    }

}
