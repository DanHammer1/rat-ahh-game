using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections;
using System.Timers;


public class RaceFinish : NetworkBehaviour {
    BoxCollider startTrigger;
    BoxCollider finishTrigger;
    GameObject startRace;
    GameObject startText;
    GameObject finishText;
    Coroutine raceTimer;
    [SerializeField] GameObject raceTimerUI;
    RaceStart raceStart;


    void OnEnable() {
        raceStart = transform.parent.Find("RaceStart").GetComponent<RaceStart>();
        startTrigger = raceStart.transform.GetComponent<BoxCollider>();
        finishTrigger = GetComponent<BoxCollider>();
        startRace = transform.parent.gameObject;
        startText = transform.parent.Find("RaceStart/Start").gameObject;
        finishText = transform.Find("Finish").gameObject;

        raceTimer = transform.parent.Find("RaceStart").GetComponent<RaceStart>().raceTimer;
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("PlayerMouse")) {
            Debug.Log("You did it!!");
            startTrigger.enabled = false;
            finishTrigger.enabled = false;
            startText.SetActive(false);
            finishText.SetActive(false);
            raceStart.StopCoroutine(raceStart.raceTimer);
            raceTimerUI.SetActive(false);

            Player player = other.GetComponent<Player>();
            RaceLocationManager.instance?.onRaceCompleted?.Invoke();
            startRace.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }

    IEnumerator StartRaceCoroutine(float duration) {
        float remaining = duration;
        while (remaining > 0) {
            remaining -= Time.deltaTime;
            Debug.Log(remaining);
            yield return null;
        }

        if (remaining <= 0) {
            remaining = 0;
            Debug.Log("you failed");
        }
    }
}
