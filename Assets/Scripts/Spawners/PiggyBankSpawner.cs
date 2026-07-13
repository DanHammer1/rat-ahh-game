using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PiggyBankSpawner : NetworkBehaviour
{
    public static PiggyBankSpawner instance;
    public GameObject piggyBankPrefab;
    public GameObject piggyBankFracturedPrefab;
    public Vector3 spawnPos;

    void Awake()
    {
        instance = this;
        // SpawnPiggyBankRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnPiggyBankRpc()
    {
        GameObject piggyBank = Instantiate(piggyBankPrefab, spawnPos, Quaternion.identity);
    }

}
