using UnityEngine;
using Unity.Netcode;
using System;

public class RaceLocationManager : NetworkBehaviour
{
    public static RaceLocationManager instance;
    public GameObject raceLocationContainer;
    GameObject raceTimerUI;
    int numberOfLocations;
    public Action onRaceCompleted;
    public void Awake()
    {
        instance = this;
        numberOfLocations = raceLocationContainer.transform.childCount;
        raceTimerUI = GameObject.FindWithTag("RaceTimer");
        raceTimerUI.SetActive(false);
    }

    public string ChooseRandomRaceLocation()
    {
        int randomIndex = UnityEngine.Random.Range(0, numberOfLocations);
        SetAllLocationsInactive();
        Transform selectedLocation = raceLocationContainer.transform.GetChild(randomIndex);
        selectedLocation.Find("RaceStart").GetComponent<BoxCollider>().enabled = true;
        selectedLocation.Find("RaceStart/Start").gameObject.SetActive(true);
        if (randomIndex == 0)
        {
            return "test1";
        }
        else if (randomIndex == 1)
        {
            return "test2";
        }
        else if (randomIndex == 2)
        {
            return "test3";
        }
        else if (randomIndex == 3)
        {
            return "test4";
        }
        return "error selecting race location";
    }

    public void SetAllLocationsInactive()
    {
        for (int i = 0; i < numberOfLocations; i++)
        {
            raceLocationContainer.transform.GetChild(i).Find("RaceStart").GetComponent<BoxCollider>().enabled = false;
            raceLocationContainer.transform.GetChild(i).Find("RaceFinish").GetComponent<BoxCollider>().enabled = false;
        }
    }
}