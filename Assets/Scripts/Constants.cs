using Unity.VisualScripting;
using UnityEngine;

public class Constants : MonoBehaviour {
    public static Constants instance;

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
    public static float maxRatAbilityHunterShakeMeter = 20f;
    public static float ratCameraFOV = 60f;
    public static float ratMaxCameraThirdPersonRadius = 0.8f;
    public static float ratDashAbilityPower = 16f;
    public static float ratInvisibilityAbilityCooldown = 20f; // 60f after testing is done
    public static float ratInvisibilityAbilityDuration = 7f;
    public static float ratInvisibilityAbilityVignetteFadeDuration = 0.5f;
    # endregion

    # region "Hunter"
    public static float hunterMoveSpeed = 1.3f;
    public static float crawlSpeedMultiplier = 0.4f;
    public static float hunterJumpForce = 0.3f;
    public static float hunterFallMultiplier = 0.7f;
    public static float hunterAscendMultiplier = 0.7f;
    public static float hunterCameraFOV = 60f;
    public static float hunterMaxCameraThirdPersonRadius = 0.4f;
    public static float boxColliderStandingSizeX = 0.1f;
    public static float boxColliderStandingSizeY = 0.8f;
    public static float boxColliderStandingSizeZ = 0.062937f;
    public static float boxColliderStandingCenterY = 0.4f;
    public static float boxColliderCrawlingSizeY = 0.12f;
    public static float boxColliderCrawlingSizeZ = 0.062937f;
    public static float boxColliderCrawlingCenterY = 0.04f;
    # endregion

    # region "Camera"
    public static float cameraCollisionRadius = 0.5f;
    # endregion

    # region "Misc"
    public static float respawnTime = 10;
    public static float cheeseSpawnInterval = 30;
    public static float coinSpawnInterval = 30;
    public static float piggyBankBreakSpeed = 2.5f;
    public static int piggyBankMinCoinsSpawned = 2;
    public static int piggyBankMaxCoinsSpawned = 4;
    public static float piggyBankDespawnTime = 6f;
    public static int maxObjectives = 3;
    public static float poisonDPS = 15f;
    # endregion

    void Awake() {
        instance = this;
    }
}
