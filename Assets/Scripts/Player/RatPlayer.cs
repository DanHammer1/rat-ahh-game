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
    public int lives = 1;


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        GameObject heartsContainer = GameObject.Find("HeartsContainer");
        lives = 2;
        if (SceneManager.GetActiveScene().name == "Game") {
            heartsContainer.GetComponent<HeartsContainer>().DrawHearts();
        }
    }

    protected override void Update() {
        base.Update();
    }
}