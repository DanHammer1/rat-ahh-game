using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;


public class DeliveryObjective : Objective {
    public GameObject coin;
    private bool conditionCleared;
    List<string> locationString;

    public DeliveryObjective() : base("Deliver one coin to the garbage container") {
        completionScore = ObjectiveScores.deliveryScore;
        objectiveIcon = Assets.instance.deliveryObjectiveIcon;

        locationString = CoinDeliveryLocationManager.instance.ChooseRandomCoinDeliveryLocation();
        objectiveText = $"Coin Delivery: {locationString[1]}";

        CoinSpawner.instance.onCoinDelivered += () => {
            conditionCleared = true;
        };
    }

    public override bool CheckConditionCleared() {
        return conditionCleared;
    }
    public override string GetDialogueText() {
        string text = "I WANNA BUY RAT HOOKERS!!! BREAK HUNTERS PIGGY BANK AND BRING MONEY TO THE " + locationString[0] + "!!!";
        return text;
    }
}