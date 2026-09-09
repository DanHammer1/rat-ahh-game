using UnityEngine;
using Unity.Netcode;

public class GhostMovement : NetworkBehaviour {
    public NetworkVariable<NetworkObjectReference> itemBeingPossessedObject = new NetworkVariable<NetworkObjectReference>();

    void FixedUpdate() {
        // if (Player.localPlayer != null) {
        //     eyePosition = Player.localPlayer.gameObject.transform.Find("EyePosition");
        // }
        if (!IsOwner || Player.localPlayer.dead || !itemBeingPossessedObject.Value.TryGet(out NetworkObject itemBeingPossessed)) return;

        transform.position = itemBeingPossessed.transform.position;

        if (Input.GetKeyDown(KeyCode.Z)) {
            Vector3 direction = Camera.main.transform.forward;
            PossessedJumpRpc(direction);
            // itemBeingPossessed.GetComponent<Rigidbody>().AddForce(direction * 5, ForceMode.Impulse);
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            GetComponent<RatPlayer>().unPossessedItem.Invoke();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PossessedJumpRpc(Vector3 direction) {
        if (!itemBeingPossessedObject.Value.TryGet(out NetworkObject itemBeingPossessed)) return;
        itemBeingPossessed.GetComponent<Rigidbody>().AddForce(direction * 5, ForceMode.Impulse);
    }
}