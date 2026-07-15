using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveSpawner : MonoBehaviour
{
    public Action<string> OnObjectiveCreated;
    private List<Func<Objective>> objectiveTypeList = new() 
    {
        () => new CheeseObjective(),
        () => new DeliveryObjective(),
        () => new AbilityObjective()
    };

    void Start() {
        GameObject mamaRat = GameObject.FindWithTag("MamaRat");
        if (mamaRat == null) return;

        MamaRat mamaRatScript = mamaRat.GetComponent<MamaRat>();
        mamaRatScript.onInteraction += () => CreateRandomObjective();
    }

    public void CreateRandomObjective() {
        if (ProgressManager.instance.objectives.Count > 0) {
            OnObjectiveCreated?.Invoke("Do your objective bruh.");
            return;
        }

        Objective randomObjective = objectiveTypeList[UnityEngine.Random.Range(0, objectiveTypeList.Count)]?.Invoke();
        ProgressManager.instance.objectives.Add(randomObjective);
        
        OnObjectiveCreated?.Invoke(randomObjective.GetDialogueText());
    }
}
