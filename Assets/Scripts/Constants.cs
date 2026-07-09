using Unity.VisualScripting;
using UnityEngine;

public static class Constants
{
    public static float mouseSensitivity = 1f;

    #region "Rat"
    public static float ratMoveSpeed = 1.3f;
    public static float carryingCoinMoveSpeedMultiplier = 0.8f;
    public static float ratJumpForce = 0.2f;
    public static float ratFallMultiplier = 0.8f;
    public static float ratAscendMultiplier = 0.8f;
    public static float ratAbilityDuration = 0.4f;
    public static float ratAbilityClingRange = 0.19f;
    public static float maxRatAbilityCooldown = 10f;
    public static float maxRatAbilityHumanShakeMeter = 20f;
    public static float ratCameraFOV = 60f;
    public static float ratMaxCameraThirdPersonRadius = 0.8f;
    # endregion

    # region "Human"
    public static float humanMoveSpeed = 1.3f;
    public static float humanJumpForce = 0.3f;
    public static float humanFallMultiplier = 0.7f;
    public static float humanAscendMultiplier = 0.7f;
    public static float humanCameraFOV = 60f;
    public static float humanMaxCameraThirdPersonRadius = 0.4f;
    # endregion

    # region "Camera"
    public static float cameraCollisionRadius = 0.5f;
    # endregion

    # region "Misc"
    public static float respawnTime = 10;
    public static float cheeseSpawnInterval = 30;
    # endregion
}
