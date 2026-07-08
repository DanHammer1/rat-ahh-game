using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Cheese : NetworkBehaviour, IInteractable
{
    public bool playerInRange = false;
    private RatPlayer localPlayerInRange;

    public Action onDestroyed;
    public Action onSpawned;
    public Action onPlayerSeesObject;

    private float eatProgress = 0;
    private float totalInteractionTime = 5f;

    public override void OnNetworkSpawn() {
        onSpawned += () => ObjectManager.MakeObjectSpectral(transform.Find("Renderer").gameObject);
        onSpawned += () => Timer.CreateTimer(1000, Timer.OnFinish.DESTROY, () => 
            ObjectManager.TakeAwaySpectral(transform.Find("Renderer").gameObject),
            "Spectral Effect removal for cheese timer.");

        onPlayerSeesObject += () => ObjectManager.TakeAwaySpectral(transform.Find("Renderer").gameObject);
        onSpawned?.Invoke();
    }

    void Update()
    {
        ((IInteractable)this).TryInteract();

        if (ObjectManager.CheckPlayerSeesObject(this.gameObject)) {
            onPlayerSeesObject?.Invoke();
        };
    }

    public string GetInteractionPromptText() {
        return "Hold E to eat Cheese.";
    }

    public void Interact() {
        Player.localPlayer.score += ObjectiveScores.cheeseScore;
        Player.localPlayer.scoreText.text = $"Score: {Player.localPlayer.score}";
        DespawnServerRpc();
    }

    public void UpdateProgress() {
        eatProgress += Time.deltaTime / totalInteractionTime;
    }

    public float GetProgress() {
        return eatProgress;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            onDestroyed?.Invoke();
            NetworkObject.Despawn();
        }
    }
}