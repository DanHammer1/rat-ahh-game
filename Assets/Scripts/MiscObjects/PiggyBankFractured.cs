using UnityEngine;
using Unity.Netcode;

public class PiggyBankFractured : NetworkBehaviour {
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void DestroyRpc() {
        Destroy(this.gameObject);
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (!IsServer) return;
        Timer.CreateTimer(Constants.piggyBankDespawnTime, Timer.OnFinish.DESTROY,
            () => { DestroyRpc(); });
    }
}
