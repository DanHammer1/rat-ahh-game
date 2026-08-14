using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.Rendering;

public class MatchSettingsButton : NetworkBehaviour, IInteractable {
    [SerializeField] private bool showInteractionUI = true;

    public bool ShowInteractionUI => showInteractionUI;
    private float interactionProgress = 0;
    private float interactionCompletionTime = 0f;
    private bool interactable = true;
    public GameObject matchSettingsUI;
    Movement movement;

    public enum State {
        OPEN,
        CLOSED
    }

    public State settingsUI = State.CLOSED;

    public Action onSettingsClosed;
    public Action onSettingsOpened;

    public String GetInteractionPromptText() {
        switch (settingsUI) {
            case State.CLOSED:
                return "Press E to open match settings.";
            case State.OPEN:
                return "Only one person can change settings at a time.";
            default:
                return "match settings has no state.";
        }
    }
    public void Interact() {
        matchSettingsUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        showInteractionUI = false;
        InteractSettingsRpc();
        interactable = false;
        movement = Player.localPlayer.GetComponent<Movement>();
        movement.isMovementLocked = true;
    }

    public void CloseMatchSettings() {
        matchSettingsUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        showInteractionUI = true;
        InteractSettingsRpc();
        interactable = true;
        movement = Player.localPlayer.GetComponent<Movement>();
        movement.isMovementLocked = false;
    }

    public void OnInteractingExit() {
        interactionProgress = 0;
    }

    public void UpdateProgress() {
        if (!interactable) return;

        if (interactionCompletionTime == 0) interactionProgress = 1;
        else interactionProgress += Time.deltaTime / interactionCompletionTime;
    }

    public float GetProgress() {
        return interactionProgress;
    }

    public void SwitchSettingsState() {
        if (settingsUI == State.OPEN) {
            settingsUI = State.CLOSED;
            onSettingsClosed?.Invoke();
        } else {
            settingsUI = State.OPEN;
            onSettingsOpened?.Invoke();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void InteractSettingsRpc() {
        InteractSettingsClientRpc();
    }

    [ClientRpc]
    public void InteractSettingsClientRpc() {
        SwitchSettingsState();
    }

    void Update() {
        if (settingsUI == State.CLOSED) {
            ((IInteractable)this).TryInteract();
        }

        if (Input.GetKey(KeyCode.Escape) && settingsUI == State.OPEN) {
            CloseMatchSettings();
        }
    }
}