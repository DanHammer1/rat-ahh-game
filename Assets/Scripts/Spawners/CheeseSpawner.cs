using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using System;

public class CheeseSpawner : NetworkBehaviour
{
    public static CheeseSpawner instance;
    public GameObject cheesePrefab;
    public List<GameObject> cheeseSpawnLocations;
    public NetworkList<NetworkObjectReference> takenSpawnLocations;

    public Action onCheeseObtained;

    void Awake() {
        instance = this;

        cheeseSpawnLocations = new List<GameObject>();
        GameObject cheeseSpawnLocationParent = GameObject.FindWithTag("CheeseSpawnParent");

        foreach (Transform spawnLocation in cheeseSpawnLocationParent.transform) {
            cheeseSpawnLocations.Add(spawnLocation.gameObject);
        }
        takenSpawnLocations = new NetworkList<NetworkObjectReference>();
    }

    void Start() {
        if (!IsServer) return;

        Timer.CreateTimer(30, Timer.OnFinish.REPEAT, () => SpawnRandomCheeseRpc(), "Cheese spawn repeating timer");
    }

    List<GameObject> GetVacantCheeseSpots() {
        List<GameObject> vacantSpots = new List<GameObject>();
        foreach (GameObject spawnLocation in cheeseSpawnLocations) {
            bool taken = false;
            foreach (GameObject cheese in takenSpawnLocations) {
                if (cheese.transform.position.Equals(spawnLocation.transform.position)) {
                    taken = true;
                }
            }
            if (!taken) vacantSpots.Add(spawnLocation);
        }
        return vacantSpots;
    }

    public Vector3 GetVacantCheeseSpot() {
        List<GameObject> vacantSpots = GetVacantCheeseSpots();

        if (vacantSpots.Count == 0) return Vector3.zero;

        GameObject randVacantSpot = vacantSpots[UnityEngine.Random.Range(0, vacantSpots.Count)];
        return randVacantSpot.transform.position;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnRandomCheeseRpc() {
        Vector3 vacantSpot = GetVacantCheeseSpot();

        if (vacantSpot == Vector3.zero) {
            return;
        }

        GameObject cheese = Instantiate(cheesePrefab, vacantSpot, Quaternion.identity);
        cheese.GetComponent<NetworkObject>().Spawn();

        takenSpawnLocations.Add(cheese);
        cheese.GetComponent<Cheese>().onDestroyed += () => takenSpawnLocations.Remove(cheese);
    }

    public IEnumerator ForceObtainRandomCheeseOverTime() {
        if (takenSpawnLocations.Count == 0) {
            SpawnRandomCheeseRpc();
        }

        while (takenSpawnLocations.Count == 0) {
            yield return null;
        }

        onCheeseObtained?.Invoke();
    }

    public void ForceObtainRandomCheese() {
        StartCoroutine(ForceObtainRandomCheeseOverTime());
    }

    public GameObject GetRandomCheese() {
        if (takenSpawnLocations.Count == 0) {
            return null;
        }

        return takenSpawnLocations[UnityEngine.Random.Range(0, takenSpawnLocations.Count)];
    }
}