using UnityEngine;
using Unity.Netcode;

public class CheeseObjective : Objective
{
    public GameObject cheese;

    public CheeseObjective() : base("Get Cheese.") {
        this.cheese = GameObject.FindWithTag("Cheese"); 
    }

    public override bool CheckConditionCleared() {
        return (cheese == null);
    }
}
