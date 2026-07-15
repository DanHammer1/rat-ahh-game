using UnityEngine;
using Unity.Netcode;
using System;

public abstract class Objective
{
    public string objectiveText;
    public Action onConditionCleared;
    protected int completionScore; 
    
    public Objective(string objectiveText) {
        this.objectiveText = objectiveText;
        onConditionCleared += () => Player.localPlayer.AddScoreServerRpc(completionScore);
        onConditionCleared += () => ProgressManager.instance.objectives.Remove(this);
    }

    public abstract bool CheckConditionCleared();
    public abstract string GetDialogueText();
}