using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode.Components;

public abstract class Item : NetworkBehaviour, IInteractable
{
    private float pickUpProgress = 0;
    private float totalInteractionTime = 0.7f;
    public float cooldown;
    private NetworkVariable<NetworkObjectReference> humanPlayerRef = new NetworkVariable<NetworkObjectReference>();
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
            if (!humanPlayerRef.Value.TryGet(out NetworkObject humanPlayer)) return false;
            bool isCarrying = (humanPlayer == Player.localPlayer.NetworkObject);
            return Input.GetMouseButtonDown(0) && isEquipped.Value && isCarrying;
            });
        
        useTimer.AddProgressionCondition(() => isEquipped.Value);
    }

    void Update()
    {
        ((IInteractable)this).TryInteract();

        if (!humanPlayerRef.Value.TryGet(out NetworkObject humanPlayer) || !isEquipped.Value) return;

        if (Player.localPlayer && GameManager.GetLocalRole() != GameManager.PlayerRole.HUNTER) return;
        
        if (Input.GetKeyDown(KeyCode.Q)) {
            SetIsEquippedRpc(false);
            GetComponent<NetworkTransform>().enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
            pickUpProgress = 0;

            Collider[] colliders = GetComponents<Collider>();

            foreach (Collider collider in colliders) {
                collider.enabled = true;
            }

            ((HumanPlayer)(Player.localPlayer)).SetCarryingItemRpc(false);
        }
    }

    void LateUpdate() {
        if (!humanPlayerRef.Value.TryGet(out NetworkObject humanPlayer) || !isEquipped.Value) return;
        Transform humanHand = humanPlayer.transform.Find("Armature/Hip/Spine/Upper Arm.R/Lower Arm.R/Hand.R/Hand.R_end");

        Transform parentObject = humanHand.Find(parentGameObjectName);
        if (parentObject == null) return;

        transform.position = parentObject.position;
        transform.rotation = parentObject.rotation;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetIsEquippedRpc(bool state) {
        isEquipped.Value = state;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateHumanPlayerRefRpc(NetworkObjectReference playerRef) {
        if (playerRef.TryGet(out NetworkObject player)) {
            humanPlayerRef.Value = playerRef;
        }
    }

    public bool CheckExtraInteractionConditions() {
        return (GameManager.GetLocalRole() == GameManager.PlayerRole.HUNTER && !((HumanPlayer)(Player.localPlayer)).isCarryingItem.Value);
    }

    public abstract string GetInteractionPromptText();

    public void Interact() {
        GetComponent<Rigidbody>().useGravity = false;

        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider collider in colliders) {
            collider.enabled = false;
        }

        SetIsEquippedRpc(true);

        ((HumanPlayer)Player.localPlayer).SetCarryingItemRpc(true);
        UpdateHumanPlayerRefRpc(Player.localPlayer.NetworkObject);

        GetComponent<NetworkTransform>().enabled = true;
    }

    public void UpdateProgress() {
        pickUpProgress += Time.deltaTime / totalInteractionTime;
    }

    public float GetProgress() {
        return pickUpProgress;
    }

    public abstract void UseItem();

    public void OnInteractingExit()
    {
        pickUpProgress = 0;
    }
}