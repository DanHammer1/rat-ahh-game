using UnityEngine;
using Unity.Netcode;

public class DeliveryObjective : Objective
{
    public GameObject coin;
    private bool conditionCleared;
    string locationString;

    public DeliveryObjective() : base("Deliver one coin to the garbage container")
    {
        completionScore = Constants.coinObjectiveCompletionScore;

        locationString = CoinDeliveryLocationManager.instance.ChooseRandomCoinDeliveryLocation();
        objectiveText = $"Deliver one coin to the {locationString.ToLower()}";

        // CoinSpawner.instance.ForceObtainRandomCoin();
        CoinSpawner.instance.onCoinDelivered += () =>
        {
            conditionCleared = true;
            // coin = CoinSpawner.instance.GetRandomCoin();
            // ObjectManager.MakeObjectSpectral(coin.transform.Find("Renderer").gameObject);
            // Timer.CreateTimer(Constants.coinSpawnInterval, Timer.OnFinish.DESTROY, () =>
            //     { if (this == null) return; ObjectManager.TakeAwaySpectral(coin.transform.Find("Renderer").gameObject); },
            //     "Spectral Effect removal for coin timer.", null, coin);
        };
    }

    public override bool CheckConditionCleared()
    {
        return conditionCleared;
    }
    public override string GetDialogueText()
    {
        string text = "I WANNA BUY RAT HOOKERS!!! BREAK HUNTERS PIGGY BANK AND BRING MONEY TO THE " + locationString + "!!!";
        return text;
    }
}