using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.Collections;
using System.Collections;

public class AbilityObjective : Objective
{
    private bool completed = false;

    public AbilityObjective() : base("Use cling ability on a Hunter.")
    {
        completionScore = Constants.abilityObjectiveCompletionScore;
        HumanPlayer.onHumanClung += () => completed = true;
    }

    public override bool CheckConditionCleared()
    {
        return (completed);
    }

    public override string GetDialogueText() {
        return "LETS DO SOME TROLLING!!! USE YOUR T ABILITY TO JUMP ON THE HUNTERS FACE!!!";
    }
}