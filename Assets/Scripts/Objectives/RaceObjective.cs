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
        objectiveText = $"Race: {locationString.ToLower()}";

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
        string text = $"SHOW ME YOUR SPEEDRUN LINES!! RACE FROM THE {locationString}";
        return text;
    }
}