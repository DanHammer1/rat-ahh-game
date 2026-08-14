using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using System.Collections;
using Unity.Collections;
using UnityEngine.SceneManagement;
using FMODUnity;
public class SwitchRole : NetworkBehaviour {

    GameManager.PlayerRole role;
    void Awake() {
        if (gameObject.name == "BecomeHunterTrigger") {
            role = GameManager.PlayerRole.HUNTER;
        } else {
            role = GameManager.PlayerRole.HIDER;
        }
    }

    void SwitchRoles() {
        if (!IsServer) return;

        GameManager.PlayerRole playerRole = GameManager.GetLocalRole();
        if (playerRole == role) {
            return;
        }
        ulong clientId = GameManager.GetLocalId();

        NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
        client.PlayerObject.Despawn(true);
        GameManager.Instance.SpawnPlayer(role, clientId);
    }

    void OnTriggerEnter() {
        SwitchRoles();
    }
}