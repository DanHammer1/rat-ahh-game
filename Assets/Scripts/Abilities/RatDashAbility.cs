using UnityEngine;

public class RatDashAbility : Ability
{
    public override void ExecuteAbility() {
        GetComponent<Rigidbody>().AddForce(transform.forward * Constants.ratDashAbilityPower, ForceMode.Impulse);
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