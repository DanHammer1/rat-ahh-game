using UnityEngine;
using Unity.Netcode;
using System;

public class PoisonGasHitbox : NetworkBehaviour {
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("PlayerMouse")) {
            PoisonGasDamage poisonGasDamage = other.GetComponent<PoisonGasDamage>();
            poisonGasDamage.poisonZonesCount++;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("PlayerMouse")) {
            PoisonGasDamage poisonGasDamage = other.GetComponent<PoisonGasDamage>();
            poisonGasDamage.poisonZonesCount = Math.Max(poisonGasDamage.poisonZonesCount - 1, 0);

        }
    }
}