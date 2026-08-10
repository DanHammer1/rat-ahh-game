using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ObjectiveSpawner : MonoBehaviour {
    public Action<string> OnObjectiveCreated;
    private List<Type> objectiveTypeList = new()
    {
        typeof(CheeseObjective),
        typeof(DeliveryObjective),
        // typeof(AbilityObjective),
        typeof(RaceObjective),
    };

    void Start() {
        GameObject mamaRat = GameObject.FindWithTag("MamaRat");
        if (mamaRat == null) return;

        MamaRat mamaRatScript = mamaRat.GetComponent<MamaRat>();
        mamaRatScript.onInteraction += () => CreateRandomObjective();
    }

    public void CreateRandomObjective() {
        if (ProgressManager.instance.objectives.Count >= Constants.maxObjectives) {
            OnObjectiveCreated?.Invoke("Do your objectives bruh.");
            return;
        }

        var availableObjectives = objectiveTypeList.FindAll(type => {
            return !ProgressManager.instance.objectives.Exists(o => o.GetType() == type);
        });

        if (availableObjectives.Count == 0) {
            OnObjectiveCreated?.Invoke("No more available objectives");
            return;
        }

        Type randomObjectiveType = availableObjectives[UnityEngine.Random.Range(0, availableObjectives.Count)];
        Objective randomObjective = (Objective)Activator.CreateInstance(randomObjectiveType);
        ProgressManager.instance.objectives.Add(randomObjective);

        OnObjectiveCreated?.Invoke(randomObjective.GetDialogueText());
    }
}
