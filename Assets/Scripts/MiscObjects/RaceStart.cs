using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections;
using System.Timers;


public class RaceStart : NetworkBehaviour
{
    BoxCollider startTrigger;
    BoxCollider finishTrigger;
    public Coroutine raceTimer;

    void Awake()
    {
        startTrigger = GetComponent<BoxCollider>();
        finishTrigger = transform.parent.Find("RaceFinish").GetComponent<BoxCollider>();
        if (Input.GetKeyDown(KeyCode.L) && raceTimer != null)
        {
            StopCoroutine(raceTimer);
            raceTimer = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            startTrigger.enabled = false;
            finishTrigger.enabled = true;
            raceTimer = StartCoroutine(StartRaceCoroutine(5f));
        }
    }

    IEnumerator StartRaceCoroutine(float duration)
    {
        float remaining = duration;
        while (remaining > 0)
        {
            remaining -= Time.deltaTime;
            Debug.Log(remaining);
            yield return null;
        }

        if (remaining <= 0)
        {
            remaining = 0;
            startTrigger.enabled = true;
            finishTrigger.enabled = false;
            Debug.Log("you failed");
        }
    }
}
