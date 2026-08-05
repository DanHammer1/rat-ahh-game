using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class ObjectiveSpawner : MonoBehaviour
{
    public Action<string> OnObjectiveCreated;
    private List<Func<Objective>> objectiveTypeList = new()
    {
        () => new CheeseObjective(),
        () => new DeliveryObjective(),
        () => new AbilityObjective()
    };

    void Start()
    {
        GameObject mamaRat = GameObject.FindWithTag("MamaRat");
        if (mamaRat == null) return;

        MamaRat mamaRatScript = mamaRat.GetComponent<MamaRat>();
        mamaRatScript.onInteraction += () => CreateRandomObjective();
    }

    public void CreateRandomObjective()
    {
        if (ProgressManager.instance.objectives.Count >= Constants.maxObjectives)
        {
            OnObjectiveCreated?.Invoke("Do your objectives bruh.");
            return;
        }

        var availableObjectives = objectiveTypeList.FindAll(factory =>
        {
            Type objectiveType = factory().GetType();
            return !ProgressManager.instance.objectives.Exists(o => o.GetType() == objectiveType);
        });

        if (availableObjectives.Count == 0)
        {
            OnObjectiveCreated?.Invoke("No more available objectives");
            return;
        }

        Objective randomObjective = availableObjectives[UnityEngine.Random.Range(0, availableObjectives.Count)]?.Invoke();
        ProgressManager.instance.objectives.Add(randomObjective);

        OnObjectiveCreated?.Invoke(randomObjective.GetDialogueText());
    }
}
