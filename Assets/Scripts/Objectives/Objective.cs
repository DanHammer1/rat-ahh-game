using UnityEngine;
using Unity.Netcode;
using System;

public abstract class Objective {
    public string objectiveText;
    public Action onConditionCleared;
    public Action onObjectiveCancelled;
    protected int completionScore;
    public Sprite objectiveIcon;
    public bool isObjectiveCancelled = false;

    public Objective(string objectiveText) {
        this.objectiveText = objectiveText;
        onConditionCleared += () => Player.localPlayer.AddScoreServerRpc(completionScore);
        onConditionCleared += () => GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.ObjectiveComplete);
        onConditionCleared += () => ProgressManager.instance.objectives.Remove(this);
        onObjectiveCancelled += () => ProgressManager.instance.objectives.Remove(this);
    }

    public abstract bool CheckConditionCleared();
    public bool CheckObjectiveCancelled() {
        return isObjectiveCancelled;
    }
    public abstract string GetDialogueText();
}