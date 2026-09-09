using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode.Components;

public abstract class Possessable : NetworkBehaviour, IInteractable {
    [SerializeField] private bool showInteractionUI = true;

    public bool ShowInteractionUI => showInteractionUI;
    private float pickUpProgress = 0;
    private float totalInteractionTime = 0.7f;
    public NetworkVariable<NetworkObjectReference> ratPlayerRef = new NetworkVariable<NetworkObjectReference>();


    private NetworkVariable<bool> isPossessed = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn() {
    }

    void Update() {
        ((IInteractable)this).TryInteract();

        if (NetworkManager.Singleton == null) return;
        if (!ratPlayerRef.Value.TryGet(out NetworkObject ratPlayer) || !isPossessed.Value) return;

        if (Player.localPlayer && GameManager.GetLocalRole() != GameManager.PlayerRole.HIDER) return;

        if (Input.GetKeyDown(KeyCode.Q)) {
            SetIsPossessedRpc(false);
            GetComponent<NetworkTransform>().enabled = true; // todo update
            GetComponent<Rigidbody>().useGravity = true;
            pickUpProgress = 0;

            ((RatPlayer)(Player.localPlayer)).SetIsPossessingItemRpc(false);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetIsPossessedRpc(bool state) {
        isPossessed.Value = state;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateRatPlayerRefRpc(NetworkObjectReference playerRef) {
        if (playerRef.TryGet(out NetworkObject player)) {
            ratPlayerRef.Value = playerRef;
        }
    }

    public bool CheckExtraInteractionConditions() {
        return GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER && !((RatPlayer)(Player.localPlayer)).isPossessingItem.Value;
    }

    public abstract string GetInteractionPromptText();

    public void Interact() {
        UpdateRatPlayerRefRpc(Player.localPlayer.NetworkObject);
        if (ratPlayerRef.Value.TryGet(out NetworkObject playerObj)) {
            RatPlayer ratPlayer = playerObj.GetComponent<RatPlayer>();
            ratPlayer.possessedItem.Invoke();
            SetItemBeingPossessedRpc(ratPlayerRef.Value, NetworkObject);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetItemBeingPossessedRpc(NetworkObjectReference ratPlayerRef, NetworkObjectReference itemRef) {
        if (!ratPlayerRef.TryGet(out NetworkObject playerObj))
            return;

        playerObj.GetComponent<GhostMovement>().itemBeingPossessedObject.Value = itemRef;
    }

    public void UpdateProgress() {
        pickUpProgress += Time.deltaTime / totalInteractionTime;
    }

    public float GetProgress() {
        return pickUpProgress;
    }

    public void OnInteractingExit() {
        pickUpProgress = 0;
    }
}