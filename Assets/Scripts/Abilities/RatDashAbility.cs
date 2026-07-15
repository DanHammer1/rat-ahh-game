using UnityEngine;
using Unity.Netcode;

public class RatDashAbility : Ability
{
    public ParticleSystem dashAbilityParticles;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ExecuteAbilityRpc()
    {
        GetComponent<Rigidbody>().AddForce(transform.forward * Constants.ratDashAbilityPower, ForceMode.Impulse);
        ToggleParticleSystemClientRpc(true);
        Timer.CreateTimer(0.2f, Timer.OnFinish.DESTROY,
            () => ToggleParticleSystemClientRpc(false), "Particle Stop timer.");
    }

    public override void ExecuteAbility()
    {
        ExecuteAbilityRpc();
    }

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