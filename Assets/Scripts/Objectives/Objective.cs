using UnityEngine;
using Unity.Netcode;
using System;

public abstract class Objective
{
    public string objectiveText;
    public Action onConditionCleared;
    
    public Objective(string objectiveText) {
        this.objectiveText = objectiveText;
        onConditionCleared += () => ProgressManager.instance.objectives.Remove(this);
    }

    public abstract bool CheckConditionCleared();
    public abstract string GetDialogueText();
}