using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using System.Collections;
using System;
using Unity.Cinemachine;
using Unity.VisualScripting;

[DefaultExecutionOrder(-100)]
public class PlayerSettingsUIManagerGame : NetworkBehaviour {
    public GameObject playerSettingsUI;
    public CinemachineInputAxisController cinemachineCamera;
    Movement movement;

    public string GetClientInfo(ulong clientId) {
        int index = GameManager.Instance.clientIds.IndexOf(clientId);

        if (index == -1) return "";

        FixedString32Bytes clientName = GameManager.Instance.clientNames[
            GameManager.Instance.clientIds.IndexOf(clientId)];

        int clientRoleIndex = GameManager.Instance.clientRoles[
            GameManager.Instance.clientIds.IndexOf(clientId)];

        string clientRole = ((GameManager.PlayerRole[])Enum.GetValues(typeof(GameManager.PlayerRole)))[clientRoleIndex].ToString();

        return $@"{clientId} - {clientName} - {clientRole}";
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            TogglePlayerSettingsUI();
        }
    }

    void TogglePlayerSettingsUI() {
        playerSettingsUI.SetActive(!playerSettingsUI.activeSelf);
        movement = Player.localPlayer.GetComponent<Movement>();
        Player.localPlayer.GetComponent<Player>().isInUIMenu = !Player.localPlayer.GetComponent<Player>().isInUIMenu;

        if (playerSettingsUI.activeSelf) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            movement.isMovementLocked = true;
            cinemachineCamera.enabled = false;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            movement.isMovementLocked = false;
            cinemachineCamera.enabled = true;
        }
    }

    public void Disconnect() {
        GameManager.DisconnectToMainMenu();
    }
}