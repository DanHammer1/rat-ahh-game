using UnityEngine;
using Unity.Netcode;

public class Radar : Item
{
    public override void UseItem() {
        Debug.Log("PINGASINGAS");
        ((HumanPlayer)Player.localPlayer).SetCarryingItemRpc(false);
        DespawnServerRpc();
    }

    public override string GetInteractionPromptText() {
        return "Hold E to pick up radar.";
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