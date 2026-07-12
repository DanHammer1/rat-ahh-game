using UnityEngine;
using Unity.Netcode;

public class SprayCan : Item
{
    public override void UseItem() {
        Debug.Log("Spraying");

        ((HumanPlayer)Player.localPlayer).SetCarryingItemRpc(false);
        DespawnServerRpc();
    }

    public override string GetInteractionPromptText() {
        return "Hold E to pick up spray can.";
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