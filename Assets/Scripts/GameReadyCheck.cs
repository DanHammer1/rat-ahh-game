using UnityEngine;
using System.Collections;
using System;

public class GameReadyCheck : MonoBehaviour
{
    public static Action onFirstUpdate;
    private bool hasExecuted = false;

    public void Start() {
        GameManager.Instance.sceneReady = true;
    }

    void Update() {
        if (hasExecuted) return;

        onFirstUpdate?.Invoke();
        hasExecuted = true;
    }
}