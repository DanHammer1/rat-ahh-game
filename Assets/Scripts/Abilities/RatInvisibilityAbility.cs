using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

public class RatInvisibilityAbility : Ability
{
    SkinnedMeshRenderer playerRenderer;
    public ParticleSystem invisibilityParticles;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerRenderer = transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
        invisibilityParticles = transform.Find("InvisibilityParticles").GetComponent<ParticleSystem>();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ExecuteAbilityRpc()
    {
        SetInvisibleRpc();
        Timer.CreateTimer(Constants.ratInvisibilityAbilityDuration, Timer.OnFinish.DESTROY,
            () => { SetVisibleRpc(); });
    }

    [Rpc(SendTo.Everyone)]
    void SetVisibleRpc()
    {
        playerRenderer.materials = Assets.instance.ratMaterials;
    }

    [Rpc(SendTo.Everyone)]
    void SetInvisibleRpc()
    {
        playerRenderer.materials = Assets.instance.ratTransparentMaterials;
        invisibilityParticles.Play();
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
}