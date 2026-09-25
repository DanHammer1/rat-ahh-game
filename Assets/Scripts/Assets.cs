using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FMODUnity;
using UnityEditor;
public class Assets : MonoBehaviour {
    public static Assets instance;

    #region "Ability Icons"
    [Header("Ability Icons")]
    public Sprite ratClingAbilityIcon;
    public Sprite ratDashAbilityIcon;
    public Sprite ratInvisibilityAbilityIcon;
    #endregion

    #region "Rat Materials"
    [Header("Rat Materials")]
    public Material[] ratMaterials;
    public Material[] ratTransparentMaterials;
    public Material[] ratGhostMaterials;
    #endregion

    #region "Shaders"
    [Header("Shaders")]
    public Material invisibilityMaterial;
    #endregion

    #region "Prefabs"
    [Header("Prefabs")]
    public GameObject poisonGasPrefab;
    public GameObject heartPrefab;
    #endregion

    #region "GameObjects"
    [Header("GameObjects")]
    public GameObject abilityParent;
    public GameObject ratAbilityShakeUI;
    public GameObject ratAbilitySlapPrompt;
    public GameObject playerResult;
    public GameObject endGameResults;
    public GameObject tauntsUI;
    public GameObject emotesUI;
    public GameObject objectivesUIGameObject;
    public GameObject ghostRulesUIGameObject;
    public GameObject ratPossessedJumpMeterUI;
    public GameObject scoreAddedNotice;
    public GameObject dropItemPrompt;
    public GameObject respawnPrompt;
    public GameObject hunterReleaseDoor;
    public GameObject ratNests;
    public GameObject hunterSpawnLocation;
    # endregion

    #region "Objective Icons"
    [Header("Objective Icons")]
    public Sprite clingObjectiveIcon;
    public Sprite cheeseObjectiveIcon;
    public Sprite deliveryObjectiveIcon;
    public Sprite raceObjectiveIcon;
    #endregion

    #region "Sound Effects"
    [Header("Sound Effects")]
    public EventReference ratDashAbilitySFX;
    public EventReference invisibilityEnterSFX;
    public EventReference invisibilityExitSFX;
    public EventReference crowbarSwingSFX;
    public EventReference ratDieSFX;
    public EventReference doorOpenSFX;
    public EventReference doorCloseSFX;
    public EventReference mamaRatNoiseSFX;
    public EventReference objectiveCompleteSFX;
    public EventReference piggyBankBreakSFX;
    public EventReference radarPingSFX;
    public EventReference radarUseSFX;
    public EventReference itemPickupSfx;
    public EventReference ratTauntSoftSFX;
    public EventReference ratTauntMediumSFX;
    public EventReference ratTauntLoudSFX;
    public EventReference pickupCoinSFX;
    public EventReference dropCoinSFX;
    public EventReference crowbarDamageSFX;
    public EventReference poisonDamage1SFX;
    public EventReference poisonDamage2SFX;
    public EventReference poisonDamage3SFX;
    public EventReference whistleSFX;
    public EventReference raceStartSFX;
    public EventReference raceSuccessSFX;
    public EventReference raceTicking1SFX;
    public EventReference raceTicking2SFX;
    public EventReference respawnSFX;
    public EventReference respawnGhostSFX;

    #endregion

    void Awake() {
        instance = this;
    }

    public enum SfxType {
        RatDashAbility,
        InvisibilityEnter,
        InvisibilityExit,
        CrowbarSwing,
        RatDie,
        DoorOpen,
        DoorClose,
        MamaRatNoise,
        ObjectiveComplete,
        PiggyBankBreak,
        itemPickup,
        radarUse,
        radarPing,
        ratTauntSoft,
        ratTauntMedium,
        ratTauntLoud,
        pickupCoin,
        dropCoin,
        crowbarDamage,
        poisonDamage1,
        poisonDamage2,
        poisonDamage3,
        whistle,
        raceStart,
        raceSuccess,
        raceTicking1,
        raceTicking2,
        respawn,
        respawnGhost,
    }

    public EventReference GetEventReferenceFromSfxType(SfxType type) {
        EventReference eventReference = type switch {
            SfxType.RatDashAbility => ratDashAbilitySFX,
            SfxType.InvisibilityEnter => invisibilityEnterSFX,
            SfxType.InvisibilityExit => invisibilityExitSFX,
            SfxType.CrowbarSwing => crowbarSwingSFX,
            SfxType.RatDie => ratDieSFX,
            SfxType.DoorOpen => doorOpenSFX,
            SfxType.DoorClose => doorCloseSFX,
            SfxType.MamaRatNoise => mamaRatNoiseSFX,
            SfxType.ObjectiveComplete => objectiveCompleteSFX,
            SfxType.PiggyBankBreak => piggyBankBreakSFX,
            SfxType.itemPickup => itemPickupSfx,
            SfxType.radarUse => radarUseSFX,
            SfxType.radarPing => radarPingSFX,
            SfxType.ratTauntSoft => ratTauntSoftSFX,
            SfxType.ratTauntMedium => ratTauntMediumSFX,
            SfxType.ratTauntLoud => ratTauntLoudSFX,
            SfxType.pickupCoin => pickupCoinSFX,
            SfxType.dropCoin => dropCoinSFX,
            SfxType.crowbarDamage => crowbarDamageSFX,
            SfxType.poisonDamage1 => poisonDamage1SFX,
            SfxType.poisonDamage2 => poisonDamage2SFX,
            SfxType.poisonDamage3 => poisonDamage3SFX,
            SfxType.whistle => whistleSFX,
            SfxType.raceStart => raceStartSFX,
            SfxType.raceSuccess => raceSuccessSFX,
            SfxType.raceTicking1 => raceTicking1SFX,
            SfxType.raceTicking2 => raceTicking2SFX,
            SfxType.respawn => respawnSFX,
            SfxType.respawnGhost => respawnGhostSFX,
            _ => default
        };

        return eventReference;
    }
}
