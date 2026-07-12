using Unity.VisualScripting;
using UnityEngine;

public class Constants : MonoBehaviour
{
    public static Constants instance;


    public Sprite ratClingAbilityIcon;
    public Sprite ratDashAbilityIcon;


    public static float mouseSensitivity = 1f;

    #region "Rat"
    public static float ratMoveSpeed = 1.3f;
    public static float carryingCoinMoveSpeedMultiplier = 0.8f;
    public static float ratJumpForce = 0.2f;
    public static float ratFallMultiplier = 0.8f;
    public static float ratAscendMultiplier = 0.8f;
    public static float ratAbilityDuration = 0.4f;
    public static float ratAbilityClingRange = 0.19f;
    public static float maxRatAbilityCooldown = 20f;
    public static float ratDashAbilityCooldown = 8f;
    public static float maxRatAbilityHumanShakeMeter = 20f;
    public static float ratCameraFOV = 60f;
    public static float ratMaxCameraThirdPersonRadius = 0.8f;
    public static float ratDashAbilityPower = 16f;
    # endregion

    # region "Human"
    public static float humanMoveSpeed = 1.3f;
    public static float crawlSpeedMultiplier = 0.4f;
    public static float humanJumpForce = 0.3f;
    public static float humanFallMultiplier = 0.7f;
    public static float humanAscendMultiplier = 0.7f;
    public static float humanCameraFOV = 60f;
    public static float humanMaxCameraThirdPersonRadius = 0.4f;
    public static float boxColliderStandingSizeX = 0.1f;
    public static float boxColliderStandingSizeY = 0.8f;
    public static float boxColliderStandingSizeZ = 0.062937f;
    public static float boxColliderStandingCenterY = 0.4f;
    public static float boxColliderCrawlingSizeY = 0.062937f;
    public static float boxColliderCrawlingSizeZ = 0.4f;
    public static float boxColliderCrawlingCenterY = 0.04f;
    # endregion

    # region "Camera"
    public static float cameraCollisionRadius = 0.5f;
    # endregion

    # region "Misc"
    public static float respawnTime = 10;
    public static float cheeseSpawnInterval = 30;
    public static float coinSpawnInterval = 30;
    # endregion

    void Awake()
    {
        instance = this;
    }
}
