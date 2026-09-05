using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Globalization;
using System;
using UnityEngine.UI;

public class MatchLengthInput : NetworkBehaviour {
    public TMP_InputField input;

    void Awake() {
        input.text = ProgressManager.instance.startingMatchLength.Value.ToString();
        ProgressManager.instance.startingMatchLength.OnValueChanged -= OnStartingMatchLengthChanged;
        ProgressManager.instance.startingMatchLength.OnValueChanged += OnStartingMatchLengthChanged;
    }

    public void OnEndEdit() {
        if (float.TryParse(input.text, out float newTime) && newTime > 0) {
            newTime = Mathf.Clamp(newTime, 5, 600);
            // input.text = newTime.ToString();
            ProgressManager.instance.SetMatchLengthRpc(newTime);
        } else {
            input.text = ProgressManager.instance.startingMatchLength.Value.ToString();
        }
    }

    void OnStartingMatchLengthChanged(float oldValue, float newValue) {
        input.text = newValue.ToString();
    }
}