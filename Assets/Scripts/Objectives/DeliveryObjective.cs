using UnityEngine;
using Unity.Netcode;

public class DeliveryObjective : Objective
{
    public GameObject cheese;

    public DeliveryObjective() : base("Deliver one coin to the garbage container")
    {
    }

    public override bool CheckConditionCleared()
    {
        // return coin colliding with well;
        return false;
    }
}
