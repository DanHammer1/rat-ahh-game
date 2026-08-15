using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode.Components;

public abstract class Item : NetworkBehaviour, IInteractable {
    [SerializeField] private bool showInteractionUI = true;

    public bool ShowInteractionUI => showInteractionUI;
    private float pickUpProgress = 0;
    private float totalInteractionTime = 0.7f;
    public float cooldown;
    public NetworkVariable<NetworkObjectReference> hunterPlayerRef = new NetworkVariable<NetworkObjectReference>();
    public string parentGameObjectName;

    private NetworkVariable<bool> isEquipped = new NetworkVariable<bool>(false);

    protected Timer useTimer;

    public override void OnNetworkSpawn() {
        useTimer = Timer.CreateTimer(
            cooldown, Timer.OnFinish.REPEAT, UseItem, "Item use Timer"
            ).GetComponent<Timer>();
        useTimer.SetProgress(1);

        useTimer.Subscribe(this.gameObject);
        useTimer.AddCompletionCondition(() => {
            if (NetworkManager.Singleton == null || !hunterPlayerRef.Value.TryGet(out NetworkObject hunterPlayer)) return false;
            bool isCarrying = (hunterPlayer == Player.localPlayer.NetworkObject);
            return Input.GetMouseButton(0) && isEquipped.Value && isCarrying;
        });

        useTimer.AddProgressionCondition(() => isEquipped.Value);
    }

    void Update() {
        ((IInteractable)this).TryInteract();

        if (NetworkManager.Singleton == null) return;
        if (!hunterPlayerRef.Value.TryGet(out NetworkObject hunterPlayer) || !isEquipped.Value) return;

        if (Player.localPlayer && GameManager.GetLocalRole() != GameManager.PlayerRole.HUNTER) return;

        if (Input.GetKeyDown(KeyCode.Q)) {
            SetIsEquippedRpc(false);
            GetComponent<NetworkTransform>().enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
            pickUpProgress = 0;

            ToggleCollidersRpc(true);

            ((HunterPlayer)(Player.localPlayer)).SetCarryingItemRpc(false);
        }
    }

    void LateUpdate() {
        if (NetworkManager.Singleton == null) return;
        if (!hunterPlayerRef.Value.TryGet(out NetworkObject hunterPlayer) || !isEquipped.Value) return;
        Transform hunterHand = hunterPlayer.transform.Find("Armature/Hip/Spine/Upper Arm.R/Lower Arm.R/Hand.R/Hand.R_end");

        Transform parentObject = hunterHand.Find(parentGameObjectName);
        if (parentObject == null) return;

        transform.position = parentObject.position;
        transform.rotation = parentObject.rotation;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetIsEquippedRpc(bool state) {
        isEquipped.Value = state;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateHunterPlayerRefRpc(NetworkObjectReference playerRef) {
        if (playerRef.TryGet(out NetworkObject player)) {
            hunterPlayerRef.Value = playerRef;
        }
    }

    public bool CheckExtraInteractionConditions() {
        return (GameManager.GetLocalRole() == GameManager.PlayerRole.HUNTER && !((HunterPlayer)(Player.localPlayer)).isCarryingItem.Value);
    }

    public abstract string GetInteractionPromptText();

    public void Interact() {
        GetComponent<Rigidbody>().useGravity = false;

        ToggleCollidersRpc(false);

        SetIsEquippedRpc(true);
        GameManager.PlayLocalSoundEffectInWorld(Assets.SfxType.itemPickup, Player.localPlayer.transform.position);

        ((HunterPlayer)Player.localPlayer).SetCarryingItemRpc(true);
        UpdateHunterPlayerRefRpc(Player.localPlayer.NetworkObject);

        GetComponent<NetworkTransform>().enabled = true;
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleCollidersRpc(bool state) {
        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider collider in colliders) {
            collider.enabled = state;
        }
    }

    public void UpdateProgress() {
        pickUpProgress += Time.deltaTime / totalInteractionTime;
    }

    public float GetProgress() {
        return pickUpProgress;
    }

    public abstract void UseItem();

    public void OnInteractingExit() {
        pickUpProgress = 0;
    }
}