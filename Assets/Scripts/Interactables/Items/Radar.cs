using UnityEngine;
using Unity.Netcode;

public class Radar : Item {
    public override void OnUseItem() {
        GameManager.PlayGlobalSoundEffectInWorld(Assets.SfxType.radarUse, transform.GetChild(0).position);
        RatPlayer[] ratPlayers = FindObjectsByType<RatPlayer>(FindObjectsSortMode.None);

        foreach (RatPlayer ratPlayer in ratPlayers) {
            if (ratPlayer.isGhost || ratPlayer.dead.Value) continue;

            GameObject playerObj = ratPlayer.gameObject;

            if (ratPlayer != Player.localPlayer && (playerObj.transform.position -
                Player.localPlayer.transform.position).magnitude < 30f) {

                ObjectManager.MakeObjectSpectralForEveryone(playerObj);

                Timer newTimer = Timer.CreateTimer(5, Timer.OnFinish.DESTROY, () =>
                    ObjectManager.TakeObjectSpectralForEveryone(playerObj, "OutlinedObjects"), "Spectral player removal timer.").GetComponent<Timer>();

                newTimer.Subscribe(playerObj);
            }
        }

        ((HunterPlayer)Player.localPlayer).SetCarryingItemRpc(false);
        DespawnServerRpc();
    }

    public override string GetInteractionPromptText() {
        return "Hold E to pick up radar.";
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc() {
        if (NetworkObject != null && NetworkObject.IsSpawned) {
            NetworkObject.Despawn();
        }
    }
}