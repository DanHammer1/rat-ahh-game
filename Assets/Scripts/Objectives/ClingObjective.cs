using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.Collections;
using System.Collections;

public class ClingObjective : Objective {
    private bool completed = false;

    public ClingObjective() : base($"Use cling ability on a Hunter  ({ObjectiveScores.clingScore})") {
        completionScore = ObjectiveScores.clingScore;
        objectiveIcon = Assets.instance.clingObjectiveIcon;
        HunterPlayer.onHunterClung += () => completed = true;
    }

    public override bool CheckConditionCleared() {
        return (completed);
    }

    public override string GetDialogueText() {
        return "LETS DO SOME TROLLING!!! USE YOUR T ABILITY TO JUMP ON THE HUNTERS FACE!!!";
    }
}