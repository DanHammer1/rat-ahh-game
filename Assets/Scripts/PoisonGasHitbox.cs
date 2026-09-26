using UnityEngine;
using Unity.Netcode;
using System;

public class PoisonGasHitbox : NetworkBehaviour {

    void OnTriggerStay(Collider other) {
        if (other.CompareTag("PlayerMouse")) {
            PoisonGasDamage poisonGasDamage = other.GetComponent<PoisonGasDamage>();
            poisonGasDamage.isPoisoned = true;
        }
    }
}