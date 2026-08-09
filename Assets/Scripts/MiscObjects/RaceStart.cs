using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections;
using System.Timers;
using UnityEditor.EditorTools;
using TMPro;


public class RaceStart : NetworkBehaviour
{
    BoxCollider startTrigger;
    BoxCollider finishTrigger;
    GameObject startText;
    GameObject finishText;
    GameObject raceTimerUI;
    TextMeshProUGUI raceTimerUIText;
    public Coroutine raceTimer;

    void Awake()
    {
        startTrigger = GetComponent<BoxCollider>();
        finishTrigger = transform.parent.Find("RaceFinish").GetComponent<BoxCollider>();
        startText = transform.Find("Start").gameObject;
        finishText = transform.parent.Find("RaceFinish/Finish").gameObject;
        raceTimerUI = GameObject.FindWithTag("RaceTimer");
        raceTimerUIText = raceTimerUI.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && raceTimer != null)
        {
            StopCoroutine(raceTimer);
            raceTimer = null;
            startTrigger.enabled = true;
            finishTrigger.enabled = false;
            startText.SetActive(true);
            finishText.SetActive(false);
            raceTimerUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            startTrigger.enabled = false;
            finishTrigger.enabled = true;
            startText.SetActive(false);
            finishText.SetActive(true);
            raceTimer = StartCoroutine(StartRaceCoroutine(5f));
            raceTimerUI.SetActive(true);
        }
    }

    IEnumerator StartRaceCoroutine(float duration)
    {
        float remaining = duration;
        while (remaining > 0)
        {
            remaining -= Time.deltaTime;

            int seconds = Mathf.FloorToInt(remaining);
            int milliseconds = Mathf.FloorToInt((remaining - seconds) * 100f);
            raceTimerUIText.text = $"{seconds:00}:{milliseconds:00}\nPress L to cancel";
            yield return null;
        }

        if (remaining <= 0)
        {
            remaining = 0;
            startTrigger.enabled = true;
            finishTrigger.enabled = false;
            startText.SetActive(true);
            finishText.SetActive(false);
            raceTimerUI.SetActive(false);
            Debug.Log("you failed");
        }
    }
}
