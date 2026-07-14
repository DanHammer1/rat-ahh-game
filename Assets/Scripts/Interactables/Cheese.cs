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
    private float totalInteractionTime = 10f;

    public override void OnNetworkSpawn() {
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

    public bool CheckExtraInteractionConditions() {
        return (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER);
    }

    public string GetInteractionPromptText() {
        return "Hold E to eat Cheese.";
    }

    public void Interact() {
        Player.localPlayer.EditScoreServerRpc(Player.localPlayer.score.Value + ObjectiveScores.cheeseScore);
        Player.localPlayer.scoreText.text = $"Score: {Player.localPlayer.score.Value}";
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