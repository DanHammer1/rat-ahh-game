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
        HumanPlayer human = Player.localPlayer as HumanPlayer;

        healthRatio = Player.localPlayer.health.Value / Player.localPlayer.maxHealth.Value;
        Vignette vignette;
        postProcessingSettings.Profile.TryGet(out vignette);



        if (human != null && human.isBeingClung.Value)
        {
            Debug.Log(human);
            vignette.color.value = new Color(1f, 0.2f, 0.2f); // softer red
            vignette.intensity.value = (float)human.slapCount.Value / 50;
        }
        else
        {
            vignette.color.value = Color.red;
            vignette.intensity.value = minVignetteIntensity + (maxVignetteIntensity - minVignetteIntensity) * (1 - healthRatio);
        }
    }
}
