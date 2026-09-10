using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using ParrelSync.NonCore;
using UnityEditor.Search;
using UnityEngine.SceneManagement;
using System;

public class RatPlayer : Player {
    public bool isInvisible = false;
    public bool isGhost = false;
    public NetworkVariable<int> lives;
    public Action possessedItem;
    public Action unPossessedItem;
    public NetworkVariable<bool> isPossessingItem = new NetworkVariable<bool>(false);
    PossessedMovement possessedMovement;
    public GameObject ratPossessedJumpMeterUI;


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        InitialiseRatFeatures();
        possessedItem += PossessedItem;
        unPossessedItem += UnPossessedItem;
        boxCollider = GetComponent<BoxCollider>();
        possessedMovement = GetComponent<PossessedMovement>();
        ratPossessedJumpMeterUI = Assets.instance.ratPossessedJumpMeterUI;
    }

    public void InitialiseRatFeatures() {
        if (IsOwner && SceneManager.GetActiveScene().name == "Game") {
            HeartsContainer heartsContainer = GameObject.Find("HeartsContainer").GetComponent<HeartsContainer>();
            heartsContainer.DrawHearts();
            lives.OnValueChanged -= OnLivesChanged;
            lives.OnValueChanged += OnLivesChanged;
        }
        if (IsServer) lives.Value = ProgressManager.instance.startingRatLives.Value;
    }


    public void OnLivesChanged(int oldValue, int newValue) {
        if (!IsOwner) return;
        HeartsContainer heartsContainer = GameObject.Find("HeartsContainer").GetComponent<HeartsContainer>();
        heartsContainer.DrawHearts();
    }

    protected override void Update() {
        base.Update();
    }

    void PossessedItem() {
        SetIsPossessingItemRpc(true);
        PossessedItemRpc();
    }
    void UnPossessedItem() {
        if (possessedMovement.itemBeingPossessedObject.Value.TryGet(out NetworkObject itemBeingPossessed)) {
            itemBeingPossessed.GetComponent<Possessable>().SetIsPossessedRpc(false);
        }
        hideRatPossessedJumpMeterUIRpc();
        UnPossessedItemRpc();
        SetItemBeingPossessedObjectRpc();
        SetIsPossessingItemRpc(false);
    }

    [Rpc(SendTo.Everyone)]
    void PossessedItemRpc() {
        skinnedMeshRenderer.enabled = false;
        boxCollider.enabled = false;
        movement.isMovementLocked = true;
        movement.toggleGravity = false;
    }
    [Rpc(SendTo.Everyone)]
    void UnPossessedItemRpc() {
        skinnedMeshRenderer.enabled = true;
        boxCollider.enabled = true;
        movement.isMovementLocked = false;
        movement.toggleGravity = true;

    }

    [Rpc(SendTo.Owner)]
    public void hideRatPossessedJumpMeterUIRpc() {
        ratPossessedJumpMeterUI.SetActive(false);
        Debug.Log("ran here");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetIsPossessingItemRpc(bool state) {
        isPossessingItem.Value = state;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetItemBeingPossessedObjectRpc() {
        possessedMovement.itemBeingPossessedObject.Value = default;
    }
}