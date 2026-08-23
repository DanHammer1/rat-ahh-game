using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;


public class CoinDeliveryLocationManager : NetworkBehaviour {
    public static CoinDeliveryLocationManager instance;
    public GameObject coinDeliveryLocationContainer;
    int numberOfLocations;
    public void Awake() {
        instance = this;
        numberOfLocations = coinDeliveryLocationContainer.transform.childCount;
    }

    public List<string> ChooseRandomCoinDeliveryLocation() {
        int randomIndex = Random.Range(0, numberOfLocations);
        SetAllLocationsInactive();
        GameObject selectedLocation = coinDeliveryLocationContainer.transform.GetChild(randomIndex).gameObject;
        selectedLocation.SetActive(true);
        if (randomIndex == 0) {
            return new List<string> {
                "GARAGE BIN",
                "garage bin"
            };
        } else if (randomIndex == 1) {
            return new List<string> {
                "GARAGE WASHING MACHINES",
                "washing machines"
            };
        } else if (randomIndex == 2) {
            return new List<string> {
                "SECOND FLOOR TOILET",
                "upstairs toilet"
            };
        } else if (randomIndex == 3) {
            return new List<string> {
                "VAULT BEHIND ME",
                "Mama Rat's vault"
            };
        }
        return new List<string> {
                "error selecting coin delivery location",
                "error selecting coin delivery location"
            };
    }

    public void SetAllLocationsInactive() {
        for (int i = 0; i < numberOfLocations; i++) {
            coinDeliveryLocationContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}