using UnityEngine;

public static class Constants
{
    public static float mouseSensitivity = 1f;


    #region "Rat"
    public static float ratMoveSpeed = 1.3f;
    public static float ratJumpForce = 0.2f;
    public static float ratFallMultiplier = 0.8f;
    public static float ratAscendMultiplier = 0.8f;
    public static float ratAbilityDuration = 0.4f;
    public static float ratAbilityClingRange = 0.19f;
    public static float maxRatAbilityCooldown = 10f;
    public static float maxRatAbilityHumanShakeMeter = 10f;
    # endregion

    # region "Human"
    public static float humanMoveSpeed = 1f;
    public static float humanJumpForce = 0.3f;
    public static float humanFallMultiplier = 0.7f;
    public static float humanAscendMultiplier = 0.7f;
    # endregion
}
