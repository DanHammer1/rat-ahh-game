using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Cheese : NetworkBehaviour, IInteractable
{
    // Update is called once per frame

    public bool playerInRange = false;
    private RatPlayer localPlayerInRange;

    public Action onDestroyed;

    void Update()
    {
        ((IInteractable)this).TryInteract();
    }

    public string GetInteractionPromptText() {
        return "Press E to eat Cheese.";
    }

    public void Interact() {
        Player.localPlayer.score += ObjectiveScores.cheeseScore;
        Player.localPlayer.scoreText.text = $"Score: {Player.localPlayer.score}";
        DespawnServerRpc();
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