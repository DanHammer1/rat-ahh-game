using UnityEngine;
using Unity.Netcode;

public class DeliveryObjective : Objective {
    public GameObject coin;
    private bool conditionCleared;
    string locationString;

    public DeliveryObjective() : base("Deliver one coin to the garbage container") {
        completionScore = ObjectiveScores.deliveryScore;

        locationString = CoinDeliveryLocationManager.instance.ChooseRandomCoinDeliveryLocation();
        objectiveText = $"Deliver one coin to the {locationString.ToLower()}";

        CoinSpawner.instance.onCoinDelivered += () => {
            conditionCleared = true;
        };
    }

    public override bool CheckConditionCleared() {
        return conditionCleared;
    }
    public override string GetDialogueText() {
        string text = "I WANNA BUY RAT HOOKERS!!! BREAK HUNTERS PIGGY BANK AND BRING MONEY TO THE " + locationString + "!!!";
        return text;
    }
}