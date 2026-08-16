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
public class PlayerSettingsUIManager : NetworkBehaviour {
    public TMP_InputField nameInput;
    public TextMeshPro lobbyText;
    public GameObject playerSettingsUI;
    public MatchSettingsButton matchSettingsButton;
    public CinemachineInputAxisController cinemachineCamera;
    Movement movement;


    GameManager.PlayerRole preference;
    bool hasPreference = false;


    // Update is called once per frame

    public void BecomeHider() {
        preference = GameManager.PlayerRole.HIDER;
        hasPreference = true;

        ulong clientId = NetworkManager.Singleton.LocalClientId;
        EditClientRoleServerRpc(clientId, preference);
    }

    public void BecomeHunter() {
        preference = GameManager.PlayerRole.HUNTER;
        hasPreference = true;

        ulong clientId = NetworkManager.Singleton.LocalClientId;
        EditClientRoleServerRpc(clientId, preference);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void EditClientRoleServerRpc(ulong clientId, GameManager.PlayerRole preference) {
        GameManager.Instance.clientRoles[GameManager.Instance.clientIds.IndexOf(clientId)]
                = (int)(preference);
    }
    public void UpdateName() {
        UpdateNameServerRpc(NetworkManager.Singleton.LocalClientId, nameInput.text);
        UpdateLobbyText();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void UpdateNameServerRpc(ulong clientId, FixedString32Bytes name) {
        GameManager.Instance.clientNames[
            GameManager.Instance.clientIds.IndexOf(clientId)] = name;
    }

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

    void UpdateLobbyText() {
        string wantedLobbyText = $@"<u>Players</u>\n{GetClientInfo(NetworkManager.ServerClientId)} <i>(Host)</i>\n";

        int i = 0;
        foreach (ulong clientId in GameManager.Instance.clientIds) {
            if (clientId != NetworkManager.ServerClientId) {
                wantedLobbyText += GetClientInfo(clientId) + "\n";
            }
            i++;
        }

        lobbyText.text = wantedLobbyText;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote) && matchSettingsButton.settingsUI == MatchSettingsButton.State.CLOSED) {
            TogglePlayerSettingsUI();
        }
    }

    void TogglePlayerSettingsUI() {
        playerSettingsUI.SetActive(!playerSettingsUI.activeSelf);
        movement = Player.localPlayer.GetComponent<Movement>();

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


}