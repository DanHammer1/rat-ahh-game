using UnityEngine;
using Unity.Netcode;

public class PiggyBankFractured : NetworkBehaviour
{
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    void DestroyRpc()
    {
        Destroy(this.gameObject);
    }

    void Awake()
    {
        Timer.CreateTimer(Constants.piggyBankDespawnTime, Timer.OnFinish.DESTROY,
            () => { DestroyRpc(); });
    }
}
