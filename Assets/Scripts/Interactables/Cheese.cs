using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Cheese : NetworkBehaviour, IInteractable {
    [SerializeField] private bool showInteractionUI = true;

    public bool ShowInteractionUI => showInteractionUI;
    public bool playerInRange = false;
    private RatPlayer localPlayerInRange;

    public Action onDestroyed;
    public Action onSpawned;
    public Action onPlayerSeesObject;

    private float eatProgress = 0;
    private float totalInteractionTime = 6f;

    public override void OnNetworkSpawn() {
        onPlayerSeesObject += () => ObjectManager.TakeAwaySpectral(transform.Find("Renderer").gameObject);
        onSpawned?.Invoke();
    }

    void Update() {
        ((IInteractable)this).TryInteract();

        if (ObjectManager.CheckPlayerSeesObject(this.gameObject)) {
            onPlayerSeesObject?.Invoke();
        }
    }

    public bool CheckExtraInteractionConditions() {
        return (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER);
    }

    public string GetInteractionPromptText() {
        return "Hold E to eat Cheese.";
    }

    public void Interact() {
        //todo consider below - should eating cheese always give points even if its not an objective?
        if (Player.localPlayer == null) return;
        DespawnServerRpc();
        foreach (Objective objective in ProgressManager.instance.objectives) {
            if (objective is CheeseObjective cheeseObjective) {
                cheeseObjective.isConditionCleared = true;
                return;
            }
        }
    }

    public void UpdateProgress() {
        eatProgress += Time.deltaTime / totalInteractionTime;
    }

    public float GetProgress() {
        return eatProgress;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void DespawnServerRpc() {
        if (NetworkObject != null && NetworkObject.IsSpawned) {
            onDestroyed?.Invoke();
            NetworkObject.Despawn();
        }
    }
}