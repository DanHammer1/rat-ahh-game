using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;

public class RotateAnimation : MonoBehaviour {
    [SerializeField] float rotateSpeed = 0.5f;

    RectTransform rt;

    void Start() {
        rt = GetComponent<RectTransform>();
    }

    void Update() {
        rt.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
    }
}
