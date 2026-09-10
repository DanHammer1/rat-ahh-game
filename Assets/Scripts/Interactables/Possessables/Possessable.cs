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


    public NetworkVariable<bool> isPossessed = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn() {
    }

    void Update() {
        ((IInteractable)this).TryInteract();

        if (NetworkManager.Singleton == null) return;
        if (!ratPlayerRef.Value.TryGet(out NetworkObject ratPlayer) || !isPossessed.Value) return;

        if (Player.localPlayer && GameManager.GetLocalRole() != GameManager.PlayerRole.HIDER) return;
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
        return GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER && !((RatPlayer)(Player.localPlayer)).isPossessingItem.Value && !isPossessed.Value;
    }

    public abstract string GetInteractionPromptText();

    public void Interact() {
        UpdateRatPlayerRefRpc(Player.localPlayer.NetworkObject);
        if (ratPlayerRef.Value.TryGet(out NetworkObject playerObj)) {
            RatPlayer ratPlayer = playerObj.GetComponent<RatPlayer>();
            ratPlayer.possessedItem.Invoke();
            SetItemBeingPossessedRpc(ratPlayerRef.Value, NetworkObject);
            SetIsPossessedRpc(true);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetItemBeingPossessedRpc(NetworkObjectReference ratPlayerRef, NetworkObjectReference itemRef) {
        if (!ratPlayerRef.TryGet(out NetworkObject playerObj))
            return;

        playerObj.GetComponent<PossessedMovement>().itemBeingPossessedObject.Value = itemRef;
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