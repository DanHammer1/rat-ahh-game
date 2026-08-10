using UnityEngine;
using Unity.Netcode;

public class CoinDeliveryLocationManager : NetworkBehaviour {
    public static CoinDeliveryLocationManager instance;
    public GameObject coinDeliveryLocationContainer;
    int numberOfLocations;
    public void Awake() {
        instance = this;
        numberOfLocations = coinDeliveryLocationContainer.transform.childCount;
    }

    public string ChooseRandomCoinDeliveryLocation() {
        int randomIndex = Random.Range(0, numberOfLocations);
        SetAllLocationsInactive();
        GameObject selectedLocation = coinDeliveryLocationContainer.transform.GetChild(randomIndex).gameObject;
        selectedLocation.SetActive(true);
        if (randomIndex == 0) {
            return "GARAGE BIN";
        } else if (randomIndex == 1) {
            return "GARAGE WASHING MACHINES";
        } else if (randomIndex == 2) {
            return "SECOND FLOOR TOILET";
        } else if (randomIndex == 3) {
            return "VAULT BEHIND ME";
        }
        return "error selecting coin delivery location";
    }

    public void SetAllLocationsInactive() {
        for (int i = 0; i < numberOfLocations; i++) {
            coinDeliveryLocationContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}