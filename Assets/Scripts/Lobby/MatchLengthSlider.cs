using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class MatchLengthSlider : NetworkBehaviour {
    public Slider slider;
    void Awake() {
        slider.value = ProgressManager.instance.startingMatchLength.Value;
        ProgressManager.instance.startingMatchLength.OnValueChanged -= OnStartingMatchLengthChanged;
        ProgressManager.instance.startingMatchLength.OnValueChanged += OnStartingMatchLengthChanged;
    }

    public void OnSliderValueChanged() {
        ProgressManager.instance.SetMatchLengthRpc(slider.value);
    }

    void OnStartingMatchLengthChanged(float oldValue, float newValue) {
        slider.value = newValue;
    }
}