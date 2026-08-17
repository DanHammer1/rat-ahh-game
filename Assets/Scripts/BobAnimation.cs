using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class BobAnimation : MonoBehaviour {
    [SerializeField] float bobHeight = 0.04f;
    [SerializeField] float bobSpeed = 4f;

    Vector3 startPos;

    void Start() {
        startPos = transform.position;
    }

    void Update() {
        float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0, y, 0);
    }
}
