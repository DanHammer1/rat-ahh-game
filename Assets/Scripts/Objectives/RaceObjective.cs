using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;


public class RaceObjective : Objective {
    public GameObject coin;
    private bool conditionCleared;
    List<string> locationString;

    public RaceObjective() : base("Race") {
        completionScore = ObjectiveScores.raceScore;

        locationString = RaceLocationManager.instance.ChooseRandomRaceLocation();
        objectiveText = $"Race: {locationString[1]}";

        RaceLocationManager.instance.onRaceCompleted += () => {
            conditionCleared = true;
        };
    }

    public override bool CheckConditionCleared() {
        return conditionCleared;
    }
    public override string GetDialogueText() {
        string text = $"SHOW ME YOUR SPEEDRUN LINES!! RACE FROM THE {locationString[0]}";
        return text;
    }
}