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
        GameManager.PlayerRole playerRole = GameManager.GetLocalRole();
        if (playerRole == role ||
            playerRole == GameManager.PlayerRole.HIDER && Player.localPlayer.GetComponent<RatPlayer>().isInvisible.Value == true) {
            return;
        }
        SwitchUIElements();
        SwitchRolesServerRpc(role);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SwitchRolesServerRpc(GameManager.PlayerRole role, RpcParams rpcParams = default) {
        ulong clientId = rpcParams.Receive.SenderClientId;
        GameManager.Instance.clientRoles[GameManager.Instance.clientIds.IndexOf(clientId)]
                = (int)(role);

        NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];
        Transform playerObject = client.PlayerObject.transform;
        Vector3 spawnPos = playerObject.position;
        Quaternion spawnRotation = playerObject.rotation;
        Debug.Log(spawnPos);
        client.PlayerObject.Despawn(true);
        GameManager.Instance.SpawnPlayer(role, clientId, spawnPos, spawnRotation);
    }
    void SwitchUIElements() {
        Assets.instance.abilityParent.SetActive(!Assets.instance.abilityParent.activeSelf);
        Assets.instance.tauntsUI.SetActive(!Assets.instance.tauntsUI.activeSelf);
    }

    void OnTriggerStay(Collider other) {
        if (Player.localPlayer.gameObject == other.gameObject) {
            SwitchRoles();
        }
    }
}