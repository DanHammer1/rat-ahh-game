using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;

public class RotateAnimation : MonoBehaviour {
    [SerializeField] float rotateSpeed = 0.5f;



    void Update() {
        transform.rotation *= Quaternion.Euler(0, rotateSpeed, 0);
    }
}
