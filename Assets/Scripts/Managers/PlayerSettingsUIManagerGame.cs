using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System;
using Unity.Cinemachine;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class PlayerSettingsUIManagerGame : NetworkBehaviour {
    public GameObject playerSettingsUI;
    public Button returnToLobbyButton;
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

    public override void OnNetworkSpawn() {
        returnToLobbyButton.interactable = IsServer;
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

    public void ReturnToLobby() {
        GameManager.Instance.ReturnToLobby();
    }
}