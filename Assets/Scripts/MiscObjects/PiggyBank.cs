using UnityEngine;
using System;
using Unity.Netcode;


public class PiggyBank : NetworkBehaviour
{
    public GameObject piggyBankFracturedPrefab;

    void Start()
    {
        Debug.Log(PiggyBankSpawner.instance);
        piggyBankFracturedPrefab = PiggyBankSpawner.instance?.piggyBankFracturedPrefab;
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

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}
