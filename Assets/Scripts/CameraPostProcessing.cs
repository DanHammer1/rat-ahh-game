using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraPostProcessing : MonoBehaviour
{
    CinemachineVolumeSettings postProcessingSettings;

    public float maxVignetteIntensity = 0.5f;
    public float minVignetteIntensity = 0;

    float healthRatio;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        postProcessingSettings = this.GetComponent<CinemachineVolumeSettings>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Player.localPlayer) return;


        healthRatio = Player.localPlayer.health.Value / Player.localPlayer.maxHealth.Value;

        Vignette vignette;
        postProcessingSettings.Profile.TryGet(out vignette);
        vignette.intensity.value = minVignetteIntensity + (maxVignetteIntensity - minVignetteIntensity) * (1 - healthRatio);
    }
}
