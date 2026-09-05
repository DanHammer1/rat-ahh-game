using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using System.Collections;

public class GhostShader : NetworkBehaviour {
    SkinnedMeshRenderer playerRenderer;
    float glitchThreshold = 0.1f;
    private WaitForSeconds glitchLoopWait = new WaitForSeconds(0.1f);

    void Awake() {
        playerRenderer = transform.Find("Renderer").GetComponent<SkinnedMeshRenderer>();
        playerRenderer.materials = Assets.instance.ratGhostMaterials;

        foreach (Material material in playerRenderer.materials) {
            // Debug.Log(material.HasProperty("_Amount"));
            // material.SetFloat("_Amount", 0f);
            // Debug.Log(material.GetFloat("_Amount"));
        }
    }

    void Update() {

    }

    IEnumerator Start() {
        while (true) {
            float glitchTest = Random.Range(0f, 1f);
            if (glitchTest < glitchThreshold) {
                StartCoroutine(Glitch());
            }
            yield return glitchLoopWait;
        }
    }

    IEnumerator Glitch() {
        float distance = Random.Range(0.003f, 0.007f);
        // float amplitude = Random.Range(3.5f, 4.5f);
        float speed = Random.Range(1f, 5f);

        foreach (Material material in playerRenderer.materials) {
            material.SetFloat("_Amount", 1f);
            material.SetFloat("_Distance", distance);
            // material.SetFloat("_Amplitude", amplitude);
            material.SetFloat("_Speed", speed);
        }

        yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));

        foreach (Material material in playerRenderer.materials) {
            material.SetFloat("_Amount", 0f);
        }
    }
}
