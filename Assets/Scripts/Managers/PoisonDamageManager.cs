using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using Unity.Cinemachine;
using UnityEngine.UI;

public class PoisonDamageManager : NetworkBehaviour {
    public static PoisonDamageManager Instance;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void FixedUpdate() {
        RatPlayer[] ratPlayers = FindObjectsByType<RatPlayer>(FindObjectsSortMode.None);

        foreach (RatPlayer ratPlayer in ratPlayers) {
            PoisonGasDamage poisonGasDamage = ratPlayer.GetComponent<PoisonGasDamage>();
            poisonGasDamage.isPoisoned = false;
        }
    }
}