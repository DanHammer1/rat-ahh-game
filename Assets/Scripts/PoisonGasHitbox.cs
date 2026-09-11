using UnityEngine;
using Unity.Netcode;

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
            poisonGasDamage.poisonZonesCount--;
        }
    }
}