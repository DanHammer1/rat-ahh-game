using UnityEngine;
using System;

public class ObjectiveSpawner : MonoBehaviour
{
    public Action<Objective> OnObjectiveCreated;
    
    void Start() {
        GameObject mamaRat = GameObject.FindWithTag("MamaRat");
        if (mamaRat == null) return;

        MamaRat mamaRatScript = mamaRat.GetComponent<MamaRat>();
        mamaRatScript.onInteraction += () => CreateRandomObjective();
    }

    public Objective CreateRandomObjective() {
        Objective randomObjective = new CheeseObjective();
        ProgressManager.instance.objectives.Add(randomObjective);
        
        OnObjectiveCreated?.Invoke(randomObjective);
        return randomObjective;
    }
}
