using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Globalization;
using System;
using UnityEngine.UI;

public class RatLivesInput : NetworkBehaviour {
    public TMP_InputField input;

    void Awake() {
        input.text = ProgressManager.instance.startingRatLives.Value.ToString();
        ProgressManager.instance.startingRatLives.OnValueChanged -= OnStartingRatLivesChanged;
        ProgressManager.instance.startingRatLives.OnValueChanged += OnStartingRatLivesChanged;
    }
    public void OnEndEdit() {
        if (int.TryParse(input.text, out int newValue) && newValue > 0) {
            newValue = Mathf.Clamp(newValue, 1, 10);
            SetStartingRatLives(newValue);
        } else {
            input.text = ProgressManager.instance.startingRatLives.Value.ToString();
        }
    }

    public void IncrementStartingRatLives() {
        SetStartingRatLives(Mathf.Clamp(ProgressManager.instance.startingRatLives.Value + 1, 1, 10));
    }

    public void DecrementStartingRatLives() {
        SetStartingRatLives(Mathf.Clamp(ProgressManager.instance.startingRatLives.Value - 1, 1, 10));
    }

    void SetStartingRatLives(int value) {
        if (ProgressManager.instance == null || !ProgressManager.instance.IsSpawned)
            return;

        ProgressManager.instance.SetStartingRatLivesRpc(value);
    }

    void OnStartingRatLivesChanged(int oldValue, int newValue) {
        input.text = newValue.ToString();
    }
}

