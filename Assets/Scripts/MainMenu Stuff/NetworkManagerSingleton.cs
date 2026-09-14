using Unity.Netcode;
using UnityEngine;

public class NetworkManagerSingleton : MonoBehaviour {
    public static NetworkManagerSingleton instance;

    void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}