using UnityEngine;
using System.Collections;

public class GameReadyCheck : MonoBehaviour
{
    public void Start() {
        GameManager.Instance.sceneReady = true;
    }
}