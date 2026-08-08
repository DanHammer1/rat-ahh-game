using UnityEngine;
using Unity.Netcode;

public class RaceObjective : Objective
{
    public GameObject coin;
    private bool conditionCleared;
    string locationString;

    public RaceObjective() : base("Race")
    {
        completionScore = ObjectiveScores.raceScore;

        locationString = RaceLocationManager.instance.ChooseRandomRaceLocation();
        objectiveText = $"Race {locationString}";

        RaceLocationManager.instance.onRaceCompleted += () =>
        {
            conditionCleared = true;
        };
    }

    public override bool CheckConditionCleared()
    {
        return conditionCleared;
    }
    public override string GetDialogueText()
    {
        string text = $"RACE!!! {locationString}";
        return text;
    }
}