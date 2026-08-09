using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public class PoisonGas : NetworkBehaviour
{
    Collider hitbox;
    ParticleSystem particles;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        hitbox = transform.GetComponentInChildren<BoxCollider>();
        particles = transform.GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        transform.position += transform.forward * 0.1f * Time.deltaTime;
        hitbox.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f) * Time.deltaTime;
        particles.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f) * Time.deltaTime;
    }
}