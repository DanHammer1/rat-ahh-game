using UnityEngine;
using Unity.Netcode;

public class DeliveryObjective : Objective
{
    public GameObject cheese;

    public DeliveryObjective() : base("Deliver gold to the well")
    {
        // this.coin = GameObject.FindWithTag("Coin");
        // this.well = GameObject.FindWithTag("Well");
    }

    public override bool CheckConditionCleared()
    {
        // return coin colliding with well;
        return false;
    }
}
