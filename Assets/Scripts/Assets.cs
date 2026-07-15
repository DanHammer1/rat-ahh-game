using Unity.VisualScripting;
using UnityEngine;

public class Assets : MonoBehaviour
{
    public static Assets instance;

    public Sprite ratClingAbilityIcon;
    public Sprite ratDashAbilityIcon;
    public Sprite ratInvisibilityAbilityIcon;

    void Awake()
    {
        instance = this;
    }
}
