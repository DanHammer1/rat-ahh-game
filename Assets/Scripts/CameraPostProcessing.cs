using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;

public class CameraPostProcessing : MonoBehaviour
{
    CinemachineVolumeSettings postProcessingSettings;

    public float maxVignetteIntensity = 0.5f;
    public float minVignetteIntensity = 0;
    public float maxAperture = 27f;
    public float recoveryTimer;
    public float startVignetteIntensity;
    public float startDepthOfFieldAperture;
    public float startMovementRecoveryMultiplier;
    public bool startedRecovery = false;

    float healthRatio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        postProcessingSettings = this.GetComponent<CinemachineVolumeSettings>();
        recoveryTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player.localPlayer) return;
        HumanPlayer human = Player.localPlayer as HumanPlayer;

        healthRatio = Player.localPlayer.health.Value / Player.localPlayer.maxHealth.Value;
        Vignette vignette;
        postProcessingSettings.Profile.TryGet(out vignette);
        DepthOfField depthOfField;
        postProcessingSettings.Profile.TryGet(out depthOfField);

        if (human != null && human.isBeingClung.Value)
        {
            vignette.color.value = new Color(1f, 0.2f, 0.2f); // softer red
            vignette.intensity.value = -0.5f * Mathf.Exp(-0.1f * (float)human.slapCount.Value) + 0.5f;
            depthOfField.aperture.value = maxAperture - human.slapCount.Value;
        }

        else if (human != null && human.isDizzy.Value)
        {
            if (!startedRecovery)
            {
                startVignetteIntensity = vignette.intensity.value;
                startDepthOfFieldAperture = depthOfField.aperture.value;
                recoveryTimer = 0f; // reset timer
                startMovementRecoveryMultiplier = human.movement.movementRecoveryMultiplier;
                startedRecovery = true;
            }
            human.UpdateDizzyDuration();
            float t = Mathf.Clamp01(recoveryTimer / human.dizzyDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            if (t >= 1)
            {
                human.isDizzy.Value = false;
            }

            vignette.intensity.value = Mathf.Lerp(startVignetteIntensity, 0f, easedT);
            depthOfField.aperture.value = Mathf.Lerp(startDepthOfFieldAperture, maxAperture, easedT);
            human.movement.movementRecoveryMultiplier = Mathf.Lerp(0f, 1, easedT);

            recoveryTimer += Time.deltaTime;
        }

        else
        {
            vignette.color.value = Color.red;
            vignette.intensity.value = minVignetteIntensity + (maxVignetteIntensity - minVignetteIntensity) * (1 - healthRatio);
            depthOfField.aperture.value = maxAperture;
            startedRecovery = false;
        }
    }


}
