using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class MatchLengthSlider : NetworkBehaviour {
    public Slider slider;
    void Awake() {
        slider.value = ProgressManager.instance.defaultTime.Value;
        ProgressManager.instance.defaultTime.OnValueChanged -= OnDefaultTimeChanged;
        ProgressManager.instance.defaultTime.OnValueChanged += OnDefaultTimeChanged;
    }

    public void OnSliderValueChanged() {
        ProgressManager.instance.SetMatchLengthRpc(slider.value);
    }

    void OnDefaultTimeChanged(float oldValue, float newValue) {
        slider.value = newValue;
    }
}