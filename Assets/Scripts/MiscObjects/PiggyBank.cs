using UnityEngine;
using System;
using Unity.Netcode;


public class PiggyBank : NetworkBehaviour
{
    public GameObject piggyBankFracturedPrefab;
    public GameObject coinPrefab;

    void Start()
    {
        Debug.Log(PiggyBankSpawner.instance);
        piggyBankFracturedPrefab = PiggyBankSpawner.instance?.piggyBankFracturedPrefab;
        coinPrefab = PiggyBankSpawner.instance?.coinPrefab;
        Debug.Log(piggyBankFracturedPrefab);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude >= Constants.piggyBankBreakSpeed)
        {
            Debug.Log("bank broken!");
            OnBreakRpc(transform.position, transform.rotation);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SpawnFracturedPiggyBankRpc(Vector3 position, Quaternion rotation)
    {
        Instantiate(piggyBankFracturedPrefab, position, rotation);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void OnBreakRpc(Vector3 position, Quaternion rotation)
    {
        GameObject fractured = Instantiate(piggyBankFracturedPrefab, position, rotation);
        fractured.GetComponent<NetworkObject>().Spawn();

        int coinsSpawned = UnityEngine.Random.Range(Constants.piggyBankMinCoinsSpawned, Constants.piggyBankMaxCoinsSpawned + 1);
        for (int i = 0; i < coinsSpawned; i++)
        {
            GameObject coin = Instantiate(coinPrefab, position + new Vector3(0, 0.04f * i), rotation);
            coin.GetComponent<NetworkObject>().Spawn();
        }

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }


}
