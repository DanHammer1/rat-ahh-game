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


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        InitialiseRatFeatures();
        possessedItem += PossessedItem;
        unPossessedItem += UnPossessedItem;
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
        movement.isMovementLocked = true;
        rb.useGravity = false;
        skinnedMeshRenderer.enabled = false;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;

    }
    void UnPossessedItem() {
        movement.isMovementLocked = false;
        rb.useGravity = true;
        skinnedMeshRenderer.enabled = true;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = true;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetIsPossessingItemRpc(bool state) {
        isPossessingItem.Value = state;
    }
}