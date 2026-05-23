using UnityEngine;
using Unity.Netcode;

public abstract class Objective
{
    public string objectiveText;
    
    public Objective(string objectiveText) {
        this.objectiveText = objectiveText;
    }

    public abstract bool CheckConditionCleared();

}
