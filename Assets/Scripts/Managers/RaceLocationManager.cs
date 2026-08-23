using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;


public class RaceLocationManager : NetworkBehaviour {
    public static RaceLocationManager instance;
    public GameObject raceLocationContainer;
    GameObject raceTimerUI;
    int numberOfLocations;
    public Action onRaceCompleted;
    public void Awake() {
        instance = this;
        numberOfLocations = raceLocationContainer.transform.childCount;
        raceTimerUI = GameObject.FindWithTag("RaceTimer");
        raceTimerUI.SetActive(false);
    }

    public List<string> ChooseRandomRaceLocation() {
        int randomIndex = UnityEngine.Random.Range(0, numberOfLocations);
        SetAllLocationsInactive();
        Transform selectedLocation = raceLocationContainer.transform.GetChild(randomIndex);
        selectedLocation.Find("RaceStart").GetComponent<BoxCollider>().enabled = true;
        selectedLocation.Find("RaceStart/Start").gameObject.SetActive(true);
        if (randomIndex == 0) {
            return new List<string> {
                "1ST FLOOR OFFICE TO LEDGE OVERLOOKING STAIRS",
                "office - stairs ledge"
            };
        } else if (randomIndex == 1) {
            return new List<string> {
                "1ST FLOOR BEDROOM TO GARAGE",
                "bedroom - garage"
            };
        } else if (randomIndex == 2) {
            return new List<string> {
                "KITCHEN TO TOP BUNK BED",
                "kitchen - bunk bed"
            };
        } else if (randomIndex == 3) {
            return new List<string> {
                "2ND FLOOR DESK TO OFFICE BATHROOM",
                "upstairs desk - office"
            };
        }
        return new List<string> {
            "error choosing race",
            "error choosing race",
        };
    }

    public void SetAllLocationsInactive() {
        for (int i = 0; i < numberOfLocations; i++) {
            raceLocationContainer.transform.GetChild(i).Find("RaceStart").GetComponent<BoxCollider>().enabled = false;
            raceLocationContainer.transform.GetChild(i).Find("RaceFinish").GetComponent<BoxCollider>().enabled = false;
        }
    }
}