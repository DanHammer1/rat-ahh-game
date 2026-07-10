using UnityEngine;
using Unity.Netcode;

public class RatDashAbility : Ability
{
    public ParticleSystem dashAbilityParticles;

    public override void ExecuteAbility() {
        GetComponent<Rigidbody>().AddForce(transform.forward * Constants.ratDashAbilityPower, ForceMode.Impulse);
        ToggleParticleSystemClientRpc(true);
        Timer.CreateTimer(0.2f, Timer.OnFinish.DESTROY, 
            () => ToggleParticleSystemClientRpc(false), "Particle Stop timer.");
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleParticleSystemClientRpc(bool state) {
        if (state == true)
            dashAbilityParticles.Play();
        else dashAbilityParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }



    public override bool CheckAbilityExecutable() {
        return !GetComponent<Movement>().CheckPlayerGrounded();
    }

    public override Sprite GetIconSprite() {
        return Constants.instance.ratDashAbilityIcon;
    }
    
    public override float GetAbilityCooldown() {
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