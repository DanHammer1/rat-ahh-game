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

public class RatPlayer : Player {
    public bool isInvisible = false;


    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsOwner) {
            // Assets.instance.abilityParent.SetActive(true);
            // if (Assets.instance.objectivesList) Assets.instance.objectivesList.SetActive(true);
        }
    }

    protected override void Update() {
        base.Update();
    }
}