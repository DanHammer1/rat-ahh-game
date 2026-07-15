using UnityEngine;
using Unity.Netcode;

public class RatInvisibilityAbility : Ability
{

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ExecuteAbilityRpc()
    {
        Debug.Log("Look at me I'm invisible");
    }

    public override void ExecuteAbility()
    {
        ExecuteAbilityRpc();
    }

    public override bool CheckAbilityExecutable()
    {
        return true;
    }

    public override Sprite GetIconSprite()
    {
        return Assets.instance.ratInvisibilityAbilityIcon;
    }

    public override float GetAbilityCooldown()
    {
        return Constants.ratInvisibilityAbilityCooldown;
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