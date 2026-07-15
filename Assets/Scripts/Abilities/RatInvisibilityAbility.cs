using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

public class RatInvisibilityAbility : Ability
{
    SkinnedMeshRenderer playerRenderer;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ExecuteAbilityRpc()
    {
        SetVisibleRpc(false);
        Timer.CreateTimer(Constants.ratInvisibilityAbilityDuration, Timer.OnFinish.DESTROY,
            () => { SetVisibleRpc(true); });
    }

    [Rpc(SendTo.Everyone)]
    void SetVisibleRpc(bool state)
    {
        playerRenderer.enabled = state;
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
        playerRenderer = transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
    }
}