using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections;
using System.Timers;


public class RaceFinish : NetworkBehaviour {
    BoxCollider startTrigger;
    BoxCollider finishTrigger;
    GameObject startText;
    GameObject finishText;
    Coroutine raceTimer;
    GameObject raceTimerUI;
    RaceStart raceStart;


    void Awake() {
        raceStart = transform.parent.Find("RaceStart").GetComponent<RaceStart>();
        startTrigger = raceStart.transform.GetComponent<BoxCollider>();
        finishTrigger = GetComponent<BoxCollider>();
        startText = transform.parent.Find("RaceStart/Start").gameObject;
        finishText = transform.Find("Finish").gameObject;
        raceTimerUI = GameObject.FindWithTag("RaceTimer");

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
