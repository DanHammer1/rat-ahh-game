using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using System;

public abstract class Ability : NetworkBehaviour
{
    public KeyCode hotkey;
    public int abilitySlotIndex;
    public float abilityCooldown;

    public abstract void ExecuteAbility();
    public abstract bool CheckAbilityExecutable();

    protected GameObject abilityIcon;
    protected GameObject abilityIconBackgroundOutline;
    protected Image abilityIconBackgroundOutlineImage;
    protected GameObject abilityT;
    protected TextMeshProUGUI abilityTText;
    protected GameObject abilityIconBackground;
    protected Image abilityIconBackgroundImage;
    protected TextMeshProUGUI scoreText;
    protected GameObject ratAbilityShakeUI;

    protected virtual void Update() {
        if (CheckAbilityExecutable() && Input.GetKeyDown(hotkey) && IsOwner) {
            ExecuteAbility();
        }
    }

    public override void OnNetworkSpawn() {
        abilityCooldown = 0;

        if (!IsOwner) return;
        
        abilityIcon = GameObject.FindWithTag("Ability Icon");
        abilityIconBackground = GameObject.FindWithTag("Ability Icon Background");
        abilityIconBackgroundImage = abilityIconBackground.GetComponent<Image>();
        abilityIconBackgroundOutline = GameObject.FindWithTag("Ability Icon Background Outline");
        abilityIconBackgroundOutlineImage = abilityIconBackgroundOutline.GetComponent<Image>();
        abilityT = GameObject.FindWithTag("Ability T");
        abilityTText = abilityT.GetComponent<TextMeshProUGUI>();

        ratAbilityShakeUI = GameObject.FindWithTag("Rat Ability Shake UI");

        ratAbilityShakeUI.SetActive(false);
    }
}