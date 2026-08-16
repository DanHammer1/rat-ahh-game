using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;

public class ApplyBackfaceCulling : MonoBehaviour {

    TextMeshPro textMeshPro;
    TextMeshProUGUI textMeshProUGUI;
    void Start() {
        textMeshPro = GetComponent<TextMeshPro>();
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();

        if (textMeshPro != null) {
            textMeshPro.fontMaterial.SetFloat("_CullMode", 2);
        }

        if (textMeshProUGUI != null) {
            textMeshProUGUI.fontMaterial.SetFloat("_CullMode", 2);
        }
    }
}
