using UnityEngine;
using Unity.Netcode;

public class CoinDeliveryLocationManager : NetworkBehaviour
{
    public static CoinDeliveryLocationManager instance;
    public GameObject coinDeliveryLocationContainer;
    int numberOfLocations;
    public void Awake()
    {
        instance = this;
        numberOfLocations = coinDeliveryLocationContainer.transform.childCount;
    }

    public string ChooseRandomCoinDeliveryLocation()
    {
        int randomIndex = Random.Range(0, numberOfLocations - 1);
        SetAllLocationsInactive();
        GameObject selectedLocation = coinDeliveryLocationContainer.transform.GetChild(randomIndex).gameObject;
        selectedLocation.SetActive(true);
        if (randomIndex == 0)
        {
            return "BIN IN THE GARAGE";
        }
        else if (randomIndex == 1)
        {
            return "WASHING MACHINES IN THE GARAGE";
        }
        else if (randomIndex == 2)
        {
            return "TOILET ON THE SECOND FLOOR";
        }
        else if (randomIndex == 3)
        {
            return "VAULT BEHIND ME";
        }
        return "error selecting coin delivery location";
    }

    public void SetAllLocationsInactive()
    {
        for (int i = 0; i < numberOfLocations; i++)
        {
            coinDeliveryLocationContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}