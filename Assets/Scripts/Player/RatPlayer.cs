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

public class RatPlayer : Player {
    public bool isInvisible = false;
    public bool isGhost = false;
    public NetworkVariable<int> lives;


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        InitialiseRatFeatures();
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
}