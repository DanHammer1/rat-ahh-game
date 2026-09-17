using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Cheese : NetworkBehaviour, IInteractable {
    [SerializeField] private bool showInteractionUI = true;

    public bool ShowInteractionUI => showInteractionUI;
    public bool playerInRange = false;
    private bool isEaten = false;

    public Action onDestroyed;
    public Action onSpawned;
    public Action onPlayerSeesObject;

    private float eatProgress = 0;
    private float totalInteractionTime = 6f;

    public override void OnNetworkSpawn() {
        onPlayerSeesObject += () => ObjectManager.TakeAwaySpectral(transform.Find("Renderer").gameObject);
        onSpawned += () => GameManager.Instance.spawnedObjectsToDespawn.Add(NetworkObject);
        onDestroyed += () => GameManager.Instance.spawnedObjectsToDespawn.Remove(NetworkObject);
        onSpawned?.Invoke();
    }

    void Update() {
        ((IInteractable)this).TryInteract();

        if (ObjectManager.CheckPlayerSeesObject(this.gameObject)) {
            onPlayerSeesObject?.Invoke();
        }
    }

    public bool CheckExtraInteractionConditions() {
        return GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER && !((RatPlayer)Player.localPlayer).isGhost;
    }

    public string GetInteractionPromptText() {
        return "Hold E to eat Cheese.";
    }

    public void Interact() {
        if (Player.localPlayer == null || isEaten) return;
        DespawnServerRpc();
        isEaten = true;
        foreach (Objective objective in ProgressManager.instance.objectives) {
            if (objective is CheeseObjective cheeseObjective) {
                cheeseObjective.isConditionCleared = true;
                return;
            }
        }
        // if no objective, give some points
        Player.localPlayer.AddScoreServerRpc(ObjectiveScores.baseCheeseScore);
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