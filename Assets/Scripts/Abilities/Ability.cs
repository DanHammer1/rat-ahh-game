using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System;

public abstract class Ability : NetworkBehaviour
{
    public KeyCode hotkey;
    public int abilitySlotIndex; // Range 0 - 2 All inclusive.
    protected Timer abilityTimer;

    protected Image abilityIconCooldown;
    protected Image abilityIconFilled;
    protected TextMeshProUGUI abilityHotKeyText;
    protected TextMeshProUGUI scoreText;

    public abstract void ExecuteAbility();
    public abstract bool CheckAbilityExecutable();

    public abstract Sprite GetIconSprite();
    public abstract float GetAbilityCooldown();

    protected virtual void Update() {
        if (!IsOwner) return;

        abilityIconFilled.fillAmount = abilityTimer.GetProgress();

        if (abilityTimer.GetProgress() < 1 || !CheckAbilityExecutable())
            abilityHotKeyText.color = Color.grey;
        else abilityHotKeyText.color = Color.white;
    }

    private bool AllExecutionConditionsMet() {
        return (CheckAbilityExecutable() && Input.GetKeyDown(hotkey) && IsOwner);
    }

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;

        GameObject abilityParent = GameObject.FindWithTag("AbilityParent");
        GameObject abilitySlot = abilityParent.transform.GetChild(abilitySlotIndex + 1).gameObject;
        GameObject abilityHotKey = abilitySlot.transform.Find("Hotkey").gameObject;
        abilityHotKeyText = abilityHotKey.GetComponent<TextMeshProUGUI>();

        GameObject abilityCooldownBackground = abilitySlot.transform.Find("CooldownBackground").gameObject;
        abilityIconCooldown = abilityCooldownBackground.GetComponent<Image>();

        GameObject abilityFilled = abilitySlot.transform.Find("ChargedIcon").gameObject;
        abilityIconFilled = abilityFilled.GetComponent<Image>();

        abilityTimer = Timer.CreateTimer(GetAbilityCooldown(), Timer.OnFinish.REPEAT, 
            ExecuteAbility, "Ability Timer.").GetComponent<Timer>();
        abilityTimer.AddCompletionCondition(AllExecutionConditionsMet);

        abilityTimer.SetProgress(1);

        abilityHotKeyText.text = hotkey.ToString();

        abilityIconCooldown.sprite = GetIconSprite();
        abilityIconFilled.sprite = GetIconSprite();

        abilityTimer.Subscribe(this.gameObject);
    }
}