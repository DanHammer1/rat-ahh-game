using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using Unity.Collections;
using System.Collections;

public class CheeseObjective : Objective
{
    public GameObject cheese;

    public CheeseObjective() : base("Get Cheese.")
    {
        CheeseSpawner.instance.onCheeseObtained += () => {
            cheese = CheeseSpawner.instance.GetRandomCheese();
            ObjectManager.MakeObjectSpectral(cheese.transform.Find("Renderer").gameObject);
            Timer.CreateTimer(Constants.cheeseSpawnInterval, Timer.OnFinish.DESTROY, () => 
                {if (this == null) return; ObjectManager.TakeAwaySpectral(cheese.transform.Find("Renderer").gameObject); },
                "Spectral Effect removal for cheese timer.", null, cheese);
        };
        CheeseSpawner.instance.ForceObtainRandomCheese();
    }

    public override bool CheckConditionCleared()
    {
        return (cheese == null);
    }
}