using UnityEngine;

public class Paintable : MonoBehaviour
{
    public RenderTexture[] renderTextures;
    
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material[] materials = renderer.materials;

        renderTextures = new RenderTexture[materials.Length];
        for (int i = 0; i < renderTextures.Length; i++) {
            
        }

        renderer.materials = materials;
    }
}
