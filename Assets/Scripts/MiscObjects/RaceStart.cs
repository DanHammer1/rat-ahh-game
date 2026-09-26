using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections;
using System.Timers;
using TMPro;
using UnityEngine.Rendering;


public class RaceStart : NetworkBehaviour {
    BoxCollider startTrigger;
    BoxCollider finishTrigger;
    GameObject startText;
    GameObject finishText;
    [SerializeField] GameObject raceTimerUI;
    [SerializeField] TextMeshProUGUI raceTimerUIText;
    public Coroutine raceTimer;
    void OnEnable() {
        startTrigger = GetComponent<BoxCollider>();
        finishTrigger = transform.parent.Find("RaceFinish").GetComponent<BoxCollider>();
        startText = transform.Find("Start").gameObject;
        finishText = transform.parent.Find("RaceFinish/Finish").gameObject;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.C) && raceTimer != null) {
            StopCoroutine(raceTimer);
            raceTimer = null;
            startTrigger.enabled = true;
            finishTrigger.enabled = false;
            GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.raceFail);
            startText.SetActive(true);
            finishText.SetActive(false);
            raceTimerUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == Player.localPlayer.gameObject) {
            startTrigger.enabled = false;
            finishTrigger.enabled = true;
            startText.SetActive(false);
            finishText.SetActive(true);
            raceTimer = StartCoroutine(StartRaceCoroutine(10f));
            raceTimerUI.SetActive(true);
        }
    }

    IEnumerator StartRaceCoroutine(float duration) {
        GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.raceStart);
        float remaining = duration;
        int prevSeconds = Mathf.FloorToInt(remaining);
        while (remaining > 0) {

            int seconds = Mathf.FloorToInt(remaining);
            if (prevSeconds != seconds) {
                prevSeconds = seconds;
                if (seconds % 2 == 0 && seconds != -1) GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.raceTicking1);
                else if (seconds != 0) GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.raceTicking2);
            }
            if (remaining < 0) remaining = 0;
            int milliseconds = Mathf.FloorToInt((remaining - seconds) * 100f);
            raceTimerUIText.text = $"{seconds:00}:{milliseconds:00}\nPress C to cancel";
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (remaining <= 0) {
            remaining = 0;
            startTrigger.enabled = true;
            finishTrigger.enabled = false;
            GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.raceFail);
            startText.SetActive(true);
            finishText.SetActive(false);
            raceTimerUI.SetActive(false);
        }
    }
}
