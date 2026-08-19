using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Globalization;
using System;

public class MatchLengthInput : NetworkBehaviour {
    TMP_InputField input;

    void Awake() {
        input = GetComponent<TMP_InputField>();
        input.text = ProgressManager.instance.defaultTime.Value.ToString();
        ProgressManager.instance.defaultTime.OnValueChanged -= OnMatchLengthChanged;
        ProgressManager.instance.defaultTime.OnValueChanged += OnMatchLengthChanged;
    }

    public void OnDeselectRpc() {
        if (float.TryParse(input.text, out float newTime) && newTime > 0) {
            newTime = Mathf.Clamp(newTime, 5, 600);
            // input.text = newTime.ToString();
            ProgressManager.instance.SetMatchLengthRpc(newTime);
        } else {
            input.text = ProgressManager.instance.defaultTime.Value.ToString();
        }
    }

    void OnMatchLengthChanged(float oldValue, float newValue) {
        input.text = newValue.ToString();
    }


}