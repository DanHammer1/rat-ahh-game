using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.Collections;
using System.Collections;

public class CheeseObjective : Objective {
    public GameObject cheese;
    public bool isConditionCleared = false;

    public CheeseObjective() : base($"Eat Cheese ({ObjectiveScores.cheeseScore})") {
        completionScore = ObjectiveScores.cheeseScore;
        objectiveIcon = Assets.instance.cheeseObjectiveIcon;

        // CheeseSpawner.instance.onCheeseSelected += () => {
        //     cheese = CheeseSpawner.instance.GetRandomCheese();
        // ObjectManager.MakeObjectSpectral(cheese.transform.Find("Renderer").gameObject);
        // Timer.CreateTimer(Constants.cheeseSpawnInterval, Timer.OnFinish.DESTROY, () => { 
        //     if (this == null) return; 
        //     ObjectManager.TakeAwaySpectral(cheese.transform.Find("Renderer").gameObject); 
        //     },
        //     "Spectral Effect removal for cheese timer.", 
        //     null, 
        //     cheese
        //     );
        // };
        // CheeseSpawner.instance.ForceSelectRandomCheese();
    }

    public override bool CheckConditionCleared() {
        return isConditionCleared;
    }

    public override String GetDialogueText() {
        return "IM HUNGRY!!! EAT CHEESE FOR ME!";
    }
}