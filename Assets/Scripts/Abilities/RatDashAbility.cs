using UnityEngine;
using Unity.Netcode;

public class RatDashAbility : Ability
{
    public ParticleSystem dashAbilityParticles;

<<<<<<< HEAD
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ExecuteAbilityRpc()
    {
=======
    public override void ExecuteAbility() {
>>>>>>> parent of 4d851c7 (server syncs movement instead of client WIP)
        GetComponent<Rigidbody>().AddForce(transform.forward * Constants.ratDashAbilityPower, ForceMode.Impulse);
        ToggleParticleSystemClientRpc(true);
        Timer.CreateTimer(0.2f, Timer.OnFinish.DESTROY,
            () => ToggleParticleSystemClientRpc(false), "Particle Stop timer.");
    }

<<<<<<< HEAD
    public override void ExecuteAbility()
    {
        ExecuteAbilityRpc();
    }

=======
>>>>>>> parent of 4d851c7 (server syncs movement instead of client WIP)
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleParticleSystemClientRpc(bool state)
    {
        if (state == true)
            dashAbilityParticles.Play();
        else dashAbilityParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public override bool CheckAbilityExecutable()
    {
        return true;
    }

    public override Sprite GetIconSprite()
    {
        return Assets.instance.ratDashAbilityIcon;
    }

    public override float GetAbilityCooldown()
    {
        return Constants.ratDashAbilityCooldown;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
}