using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Assets : MonoBehaviour
{
    public static Assets instance;

    #region "Ability Icons"
    public Sprite ratClingAbilityIcon;
    public Sprite ratDashAbilityIcon;
    public Sprite ratInvisibilityAbilityIcon;
    #endregion

    #region "Rat Materials"
    public Material[] ratMaterials;
    public Material[] ratTransparentMaterials;
    #endregion

    #region "Shaders"
    public Material invisibilityMaterial;
    #endregion

    void Awake()
    {
        instance = this;
    }
}
