using UnityEngine;
using Unity.Netcode;

public class Radar : Item
{
    public override void UseItem() {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players) {
            GameObject playerObj = player.gameObject;

            if (player != Player.localPlayer && (playerObj.transform.position - 
                Player.localPlayer.transform.position).magnitude < 15f) {
                
                ObjectManager.MakeObjectSpectral(playerObj);

                Timer newTimer = Timer.CreateTimer(5, Timer.OnFinish.DESTROY, () => 
                    ObjectManager.TakeAwaySpectral(playerObj), "Spectral player removal timer.").GetComponent<Timer>();

                newTimer.Subscribe(playerObj);
            }
        }

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