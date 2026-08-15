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
        // if (!IsServer) return;

        GameManager.PlayerRole playerRole = GameManager.GetLocalRole();
        if (playerRole == role) {
            return;
        }
        ulong clientId = GameManager.GetLocalId();
        EditClientRoleServerRpc(clientId, role);

        NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
        Transform playerObject = client.PlayerObject.transform;
        Vector3 spawnPos = playerObject.position;
        Quaternion spawnRotation = playerObject.rotation;
        client.PlayerObject.Despawn(true);
        GameManager.Instance.SpawnPlayer(role, clientId, spawnPos, spawnRotation);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void EditClientRoleServerRpc(ulong clientId, GameManager.PlayerRole preference) {
        GameManager.Instance.clientRoles[GameManager.Instance.clientIds.IndexOf(clientId)]
                = (int)(preference);
    }

    void OnTriggerEnter() {
        SwitchRoles();
    }
}