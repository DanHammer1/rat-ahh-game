using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Unity.VisualScripting;

public class RatInvisibilityAbility : Ability
{
    SkinnedMeshRenderer playerRenderer;
    public ParticleSystem invisibilityParticles;

    private int _voronoiIntensity = Shader.PropertyToID("_VoronoiIntensity");
    private int _vignetteIntensity = Shader.PropertyToID("_VignetteIntensity");
    private int _vignetteColor = Shader.PropertyToID("_VignetteColor");
    private const float VORONOI_INTENSITY_MAX_AMOUNT = 1.25f;
    private const float VIGNETTE_INTENSITY_MAX_AMOUNT = 1.5f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerRenderer = transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
        invisibilityParticles = transform.Find("InvisibilityParticles").GetComponent<ParticleSystem>();

        // Temporarily setting values as they are incorrect for clones for some reason
        Assets.instance.invisibilityMaterial.SetFloat(_voronoiIntensity, 0f);
        Assets.instance.invisibilityMaterial.SetFloat(_vignetteIntensity, 0f);
        Assets.instance.invisibilityMaterial.SetColor(_vignetteColor, new Color(0.302f, 0.91f, 1f));
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

        if (IsOwner)
        {
            StartCoroutine(FadeInVignette());
        }

        // Starts fade out vignette just before ability deactivates. Should be in setvisible but idk
        Timer.CreateTimer(Constants.ratInvisibilityAbilityDuration - Constants.ratInvisibilityAbilityVignetteFadeDuration, Timer.OnFinish.DESTROY,
            () =>
            {
                if (IsOwner)
                {
                    StartCoroutine(FadeOutVignette());
                }
            });
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

    private IEnumerator FadeInVignette()
    {
        // Assets.instance.invisibilityShader.SetActive(true);
        Assets.instance.invisibilityMaterial.SetFloat(_voronoiIntensity, 0f);
        Assets.instance.invisibilityMaterial.SetFloat(_vignetteIntensity, 0f);

        float elapsedTime = 0f;
        float duration = Constants.ratInvisibilityAbilityVignetteFadeDuration;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease out
            float lerpedVoronoi = Mathf.Lerp(0f, VORONOI_INTENSITY_MAX_AMOUNT, t);
            float lerpedVignette = Mathf.Lerp(0f, VIGNETTE_INTENSITY_MAX_AMOUNT, t);

            Assets.instance.invisibilityMaterial.SetFloat(_voronoiIntensity, lerpedVoronoi);
            Assets.instance.invisibilityMaterial.SetFloat(_vignetteIntensity, lerpedVignette);

            yield return null;
        }
    }

    private IEnumerator FadeOutVignette()
    {
        Assets.instance.invisibilityMaterial.SetFloat(_voronoiIntensity, VORONOI_INTENSITY_MAX_AMOUNT);
        Assets.instance.invisibilityMaterial.SetFloat(_vignetteIntensity, VIGNETTE_INTENSITY_MAX_AMOUNT);

        float elapsedTime = 0f;
        float duration = Constants.ratInvisibilityAbilityVignetteFadeDuration;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / duration;
            t = Mathf.Pow(t, 3f); // Ease in
            float lerpedVoronoi = Mathf.Lerp(VORONOI_INTENSITY_MAX_AMOUNT, 0f, t);
            float lerpedVignette = Mathf.Lerp(VIGNETTE_INTENSITY_MAX_AMOUNT, 0f, t);

            Assets.instance.invisibilityMaterial.SetFloat(_voronoiIntensity, lerpedVoronoi);
            Assets.instance.invisibilityMaterial.SetFloat(_vignetteIntensity, lerpedVignette);

            yield return null;
        }

        // Assets.instance.invisibilityShader.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
    }
}