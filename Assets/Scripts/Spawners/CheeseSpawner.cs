using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class CheeseSpawner : NetworkBehaviour
{
    public static CheeseSpawner instance;
    public GameObject cheesePrefab;
    public List<GameObject> cheeseSpawnLocations;
    public List<GameObject> takenSpawnLocations;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() {
        instance = this;

        cheeseSpawnLocations = new List<GameObject>();
        GameObject cheeseSpawnLocationParent = GameObject.FindWithTag("CheeseSpawnParent");

        foreach (Transform spawnLocation in cheeseSpawnLocationParent.transform) {
            cheeseSpawnLocations.Add(spawnLocation.gameObject);
            Debug.Log(cheeseSpawnLocations.Count + "AAAA");
        }
        takenSpawnLocations = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<GameObject> GetVacantCheeseSpots() {
        List<GameObject> vacantSpots = new List<GameObject>();
        Debug.Log(cheeseSpawnLocations.Count);
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

        GameObject randVacantSpot = vacantSpots[Random.Range(0, vacantSpots.Count)];
        return randVacantSpot.transform.position;
    }

    public void SpawnRandomCheese() {
        Vector3 vacantSpot = GetVacantCheeseSpot();

        if (vacantSpot == Vector3.zero) {
            return;
        }
        GameObject cheese = Instantiate(cheesePrefab, vacantSpot, Quaternion.identity);
        cheese.GetComponent<NetworkObject>().Spawn();

        takenSpawnLocations.Add(cheese);
        cheese.GetComponent<Cheese>().onDestroyed += () => takenSpawnLocations.Remove(cheese);
    }

    public GameObject ForceObtainRandomCheese() {
        if (takenSpawnLocations.Count == 0) {
            SpawnRandomCheese();
        }

        return GetRandomCheese();
    }

    public GameObject GetRandomCheese() {
        if (takenSpawnLocations.Count == 0) {
            return null;
        }

        return takenSpawnLocations[Random.Range(0, takenSpawnLocations.Count)];
    }
}