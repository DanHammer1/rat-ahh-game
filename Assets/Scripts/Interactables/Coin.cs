using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Coin : NetworkBehaviour, IInteractable
{
    public string GetInteractionPromptText() {
        return "Press E to pick up coin";
    }

    public void Interact() {
        Player.localPlayer.score += ObjectiveScores.deliveryScore;
        Player.localPlayer.scoreText.text = $"Score: {Player.localPlayer.score}";
        DespawnServerRpc();
    }

    public void Update() {
        ((IInteractable)this).TryInteract();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc()
    {
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}







