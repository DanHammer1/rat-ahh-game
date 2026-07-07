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
        CheeseSpawner.instance.onCheeseObtained += () => cheese = CheeseSpawner.instance.GetRandomCheese();
        CheeseSpawner.instance.ForceObtainRandomCheese();
    }

    public override bool CheckConditionCleared()
    {
        return (cheese == null);
    }
}