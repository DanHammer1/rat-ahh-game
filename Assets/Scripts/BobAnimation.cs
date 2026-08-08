using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class BobAnimation : MonoBehaviour
{
    [SerializeField] float bobHeight = 0.5f;
    [SerializeField] float bobSpeed = 4f;

    RectTransform rt;
    Vector3 startPos;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        startPos = rt.anchoredPosition;
    }

    void Update()
    {
        float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        rt.anchoredPosition = startPos + new Vector3(0, y, 0);
    }
}
