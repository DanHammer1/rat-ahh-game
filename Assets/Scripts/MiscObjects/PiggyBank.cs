using UnityEngine;
using System;
using Unity.Netcode;


public class PiggyBank : NetworkBehaviour {
    public GameObject piggyBankFracturedPrefab;
    public GameObject coinPrefab;

    void Start() {
        piggyBankFracturedPrefab = PiggyBankSpawner.instance?.piggyBankFracturedPrefab;
        coinPrefab = PiggyBankSpawner.instance?.coinPrefab;
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.relativeVelocity.magnitude >= Constants.piggyBankBreakSpeed) {
            GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.PiggyBankBreak, transform.position);
            OnBreakRpc(transform.position, transform.rotation);
        }
    }
    void SpawnObject(GameObject prefab, Vector3 position, Quaternion rotation) {
        GameObject coin = Instantiate(prefab, position, rotation);
        NetworkObject networkObject = coin.GetComponent<NetworkObject>();
        networkObject.Spawn();
        GameManager.Instance.spawnedObjectsToDespawn.Add(networkObject);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void OnBreakRpc(Vector3 position, Quaternion rotation) {
        SpawnObject(piggyBankFracturedPrefab, position, rotation);

        int coinsSpawned = UnityEngine.Random.Range(Constants.piggyBankMinCoinsSpawned, Constants.piggyBankMaxCoinsSpawned + 1);
        for (int i = 0; i < coinsSpawned; i++) {
            SpawnObject(coinPrefab, position + new Vector3(0, 0.04f * i), rotation);
        }

        if (NetworkObject != null && NetworkObject.IsSpawned) {
            GameManager.Instance.spawnedObjectsToDespawn.Remove(NetworkObject);
            NetworkObject.Despawn(true);
        }
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    void DestroyRpc() {
        Destroy(this.gameObject);
    }
}
