using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using FMODUnity;
using UnityEditor;
public class Assets : MonoBehaviour {
    public static Assets instance;

    #region "Ability Icons"
    public Sprite ratClingAbilityIcon;
    public Sprite ratDashAbilityIcon;
    public Sprite ratInvisibilityAbilityIcon;
    #endregion

    #region "Rat Materials"
    public Material[] ratMaterials;
    public Material[] ratTransparentMaterials;
    public Material[] ratGhostMaterials;
    #endregion

    #region "Shaders"
    public Material invisibilityMaterial;
    #endregion

    #region "Prefabs"
    public GameObject poisonGasPrefab;
    public GameObject heartPrefab;
    #endregion

    #region "GameObjects"
    public GameObject abilityParent;
    public GameObject ratAbilityShakeUI;
    public GameObject ratAbilitySlapPrompt;
    public GameObject playerResult;
    public GameObject endGameResults;
    public GameObject tauntsUI;
    public GameObject emotesUI;
    #endregion

    #region "Objective Icons"
    public Sprite clingObjectiveIcon;
    public Sprite cheeseObjectiveIcon;
    public Sprite deliveryObjectiveIcon;
    public Sprite raceObjectiveIcon;
    #endregion

    #region "Sound Effects"
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
        ratTauntLoud
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
            _ => default
        };

        return eventReference;
    }
}
