using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;

public class PiggyBankSpawner : NetworkBehaviour {
    public static PiggyBankSpawner instance;
    public GameObject piggyBankPrefab;
    public GameObject piggyBankFracturedPrefab;
    public GameObject coinPrefab;
    public List<GameObject> piggyBankSpawnLocations;
    public NetworkList<NetworkObjectReference> takenSpawnLocations;

    public Action onPiggyBankDelivered;



    void Awake() {
        instance = this;

        piggyBankSpawnLocations = new List<GameObject>();
        GameObject piggyBankSpawnLocationParent = GameObject.FindWithTag("PiggyBankSpawnPoints");

        foreach (Transform spawnLocation in piggyBankSpawnLocationParent.transform) {
            piggyBankSpawnLocations.Add(spawnLocation.gameObject);
        }
        takenSpawnLocations = new NetworkList<NetworkObjectReference>();
    }

    void Start() {
        if (!IsServer) return;

        SpawnRandomPiggyBankRpc();
        SpawnRandomPiggyBankRpc();

        Timer.CreateTimer(Constants.piggyBankSpawnInterval, Timer.OnFinish.REPEAT, () => SpawnRandomPiggyBankRpc(), "PiggyBank spawn repeating timer");
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Y)) {
            SpawnRandomPiggyBankRpc();
        }
    }

    List<GameObject> GetVacantPiggyBankSpots() {
        List<GameObject> vacantSpots = new List<GameObject>();
        foreach (GameObject spawnLocation in piggyBankSpawnLocations) {
            bool taken = false;
            foreach (GameObject piggyBank in takenSpawnLocations) {
                if (piggyBank.GetComponent<PiggyBank>().spawnLocation == spawnLocation) {
                    taken = true;
                }
            }
            if (!taken) vacantSpots.Add(spawnLocation);
        }
        return vacantSpots;
    }

    public GameObject GetVacantPiggyBankSpot() {
        List<GameObject> vacantSpots = GetVacantPiggyBankSpots();

        if (vacantSpots.Count == 0) return null;

        GameObject randVacantSpot = vacantSpots[UnityEngine.Random.Range(0, vacantSpots.Count)];
        return randVacantSpot;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SpawnRandomPiggyBankRpc() {
        GameObject spawnLocation = GetVacantPiggyBankSpot();
        if (spawnLocation == null) {
            return;
        }
        Vector3 vacantSpot = spawnLocation.transform.position;

        GameObject piggyBank = Instantiate(piggyBankPrefab, vacantSpot, piggyBankPrefab.transform.rotation);
        NetworkObject networkObject = piggyBank.GetComponent<NetworkObject>();
        networkObject.Spawn(); // todo
        takenSpawnLocations.Add(piggyBank);
        GameManager.Instance.spawnedObjectsToDespawn.Add(networkObject);
        piggyBank.GetComponent<PiggyBank>().spawnLocation = spawnLocation;
    }

    public IEnumerator ForceObtainRandomPiggyBankOverTime() {
        if (takenSpawnLocations.Count == 0) {
            SpawnRandomPiggyBankRpc();
        }

        while (takenSpawnLocations.Count == 0) {
            yield return null;
        }

        // onPiggyBankObtained?.Invoke();
    }

    public void ForceObtainRandomPiggyBank() {
        StartCoroutine(ForceObtainRandomPiggyBankOverTime());
    }

    public GameObject GetRandomPiggyBank() {
        if (takenSpawnLocations.Count == 0) {
            return null;
        }

        return takenSpawnLocations[UnityEngine.Random.Range(0, takenSpawnLocations.Count)];
    }
}