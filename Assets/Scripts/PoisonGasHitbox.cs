using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public class PoisonGasHitbox : NetworkBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            PoisonGasDamage poisonGasDamage = other.GetComponent<PoisonGasDamage>();
            poisonGasDamage.poisonZonesCount++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerMouse"))
        {
            PoisonGasDamage poisonGasDamage = other.GetComponent<PoisonGasDamage>();
            poisonGasDamage.poisonZonesCount--;
        }
    }
}