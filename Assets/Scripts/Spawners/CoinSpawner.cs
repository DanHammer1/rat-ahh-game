using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using System;

public class CoinSpawner : NetworkBehaviour
{
    public static CoinSpawner instance;
    public GameObject coinPrefab;
    public List<GameObject> coinSpawnLocations;
    public NetworkList<NetworkObjectReference> takenSpawnLocations;

    public Action onCoinDelivered;

    void Awake()
    {
        instance = this;

        coinSpawnLocations = new List<GameObject>();
        GameObject coinSpawnLocationParent = GameObject.FindWithTag("CoinSpawnParent");

        foreach (Transform spawnLocation in coinSpawnLocationParent.transform)
        {
            coinSpawnLocations.Add(spawnLocation.gameObject);
        }
        takenSpawnLocations = new NetworkList<NetworkObjectReference>();
    }

    void Start()
    {
        if (!IsServer) return;

        Timer.CreateTimer(30, Timer.OnFinish.REPEAT, () => SpawnRandomCoinRpc(), "Coin spawn repeating timer"); // Same as cheeseSpawner?
    }

    List<GameObject> GetVacantCoinSpots()
    {
        List<GameObject> vacantSpots = new List<GameObject>();
        foreach (GameObject spawnLocation in coinSpawnLocations)
        {
            bool taken = false;
            foreach (GameObject coin in takenSpawnLocations)
            {
                if (coin.transform.position.Equals(spawnLocation.transform.position))
                {
                    taken = true;
                }
            }
            if (!taken) vacantSpots.Add(spawnLocation);
        }
        return vacantSpots;
    }

    public Vector3 GetVacantCoinSpot()
    {
        List<GameObject> vacantSpots = GetVacantCoinSpots();

        if (vacantSpots.Count == 0) return Vector3.zero;

        GameObject randVacantSpot = vacantSpots[UnityEngine.Random.Range(0, vacantSpots.Count)];
        return randVacantSpot.transform.position;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnRandomCoinRpc()
    {
        Vector3 vacantSpot = GetVacantCoinSpot();

        if (vacantSpot == Vector3.zero)
        {
            return;
        }

        GameObject coin = Instantiate(coinPrefab, vacantSpot, Quaternion.identity);
        coin.GetComponent<NetworkObject>().Spawn();

        takenSpawnLocations.Add(coin);
        // coin.GetComponent<Coin>().onDestroyed += () => takenSpawnLocations.Remove(coin); TODO
    }

    public IEnumerator ForceObtainRandomCoinOverTime()
    {
        if (takenSpawnLocations.Count == 0)
        {
            SpawnRandomCoinRpc();
        }

        while (takenSpawnLocations.Count == 0)
        {
            yield return null;
        }

        // onCoinObtained?.Invoke(); TODO
    }

    public void ForceObtainRandomCoin()
    {
        StartCoroutine(ForceObtainRandomCoinOverTime());
    }

    public GameObject GetRandomCoin()
    {
        if (takenSpawnLocations.Count == 0)
        {
            return null;
        }

        return takenSpawnLocations[UnityEngine.Random.Range(0, takenSpawnLocations.Count)];
    }
}