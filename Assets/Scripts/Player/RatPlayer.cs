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
    public int lives;


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        InitialiseRatFeatures();
    }

    public void InitialiseRatFeatures() {
        if (!IsOwner) return;
        lives = ProgressManager.instance.startingRatLives.Value;
        GameObject heartsContainer = GameObject.Find("HeartsContainer");
        if (SceneManager.GetActiveScene().name == "Game") {
            heartsContainer.GetComponent<HeartsContainer>().DrawHearts();
        }
    }

    protected override void Update() {
        base.Update();
    }
}