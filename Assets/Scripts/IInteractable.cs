using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public interface IInteractable {
    bool ShowInteractionUI { get; }
    public void Interact();

    public string GetInteractionPromptText();

    public float GetProgress();

    public void UpdateProgress();

    public void OnInteraction() { }

    public bool CheckExtraInteractionConditions() {
        return true;
    }
    public bool CheckGlobalInteractionConditions() {
        return !Player.localPlayer.isInUIMenu;
    }

    public void TryInteract() {
        if (CheckInteractionShouldTrigger()) Interact();
        TryUpdateProgress();
    }

    public static bool CheckPlayerFacingInteractableObject() {
        if (Player.localPlayer == null) return false;

        RaycastHit hit;
        if (!HitInteractable(out hit)) return false;

        GameObject interactPrompt = GameObject.FindWithTag("InteractionPrompt");
        IInteractable implementationScript = null;
        IInteractable[] interactables = hit.collider.gameObject.GetComponents<IInteractable>();

        foreach (IInteractable interactable in interactables) {
            if (interactable.ShowInteractionUI &&
                interactable.CheckExtraInteractionConditions() &&
                interactable.CheckGlobalInteractionConditions()) {
                implementationScript = interactable;
                break;
            }
        }

        if (implementationScript == null) {
            return false;
        }
        string newInteractText = implementationScript.GetInteractionPromptText();
        interactPrompt.GetComponent<TextMeshProUGUI>().text = newInteractText;
        implementationScript.UpdateProgressBar(implementationScript.GetProgress());

        return !Player.localPlayer.dead;
    }


    public bool CheckPlayerInRange() {
        if (Player.localPlayer == null) return false;

        RaycastHit hit;
        if (!HitInteractable(out hit)) return false;

        if (hit.collider.gameObject == null || LayerMask.LayerToName(hit.collider.gameObject.layer) == "groundLayer") return false;

        GameObject hitObject = hit.collider.gameObject;
        IInteractable[] interactables = hitObject.GetComponents<IInteractable>();
        foreach (IInteractable interactable in interactables) {
            if (ReferenceEquals(interactable, this) && CheckExtraInteractionConditions()) return true;
        }

        return false;
    }
    public static bool HitInteractable(out RaycastHit hit) {
        hit = default;
        if (Physics.SphereCast(
            Player.localPlayer.viewPosition.transform.position,
            0.1f,
            PlayerCamera.mainCamera.transform.forward,
            out hit,
            1f, LayerMask.GetMask("InteractableObject", "groundLayer"))) {

            if (LayerMask.LayerToName(hit.collider.gameObject.layer).Equals("InteractableObject")) return true;
        }

        // Second check incase spherecast spawns on interactable and returns false
        if (Physics.Raycast(
            Player.localPlayer.viewPosition.transform.position,
            PlayerCamera.mainCamera.transform.forward,
            out hit,
            1f, LayerMask.GetMask("InteractableObject", "groundLayer"))) {

            if (LayerMask.LayerToName(hit.collider.gameObject.layer).Equals("InteractableObject")) return true;
        }

        return false;
    }

    public bool CheckInteractionShouldTrigger() {
        return (CheckPlayerInRange() && Input.GetKey(KeyCode.E) && GetProgress() >= 1 && CheckGlobalInteractionConditions());
    }

    public void TryUpdateProgress() {
        if (CheckPlayerInRange() && Input.GetKey(KeyCode.E)) UpdateProgress();
        else OnInteractingExit();
    }

    public void OnInteractingExit() { }

    public void UpdateProgressBar(float progress) {
        GameObject.FindWithTag("ProgressFillBar").GetComponent<Image>().fillAmount = progress;
    }

    public static void TryDisplayInteractionText() {
        GameObject interactPrompt = GameObject.FindWithTag("InteractionPrompt");
        GameObject interactBackground = GameObject.FindWithTag("InteractionPromptBackground");
        GameObject interactFillBar = GameObject.FindWithTag("ProgressFillBar");

        if (interactPrompt == null) {
            // Debug.LogError("No prompt found.");
            return;
        }

        if (!CheckPlayerFacingInteractableObject()) {
            interactPrompt.GetComponent<TextMeshProUGUI>().enabled = false;
            interactBackground.GetComponent<Image>().enabled = false;
            interactFillBar.GetComponent<Image>().enabled = false;
            return;
        }

        interactPrompt.GetComponent<TextMeshProUGUI>().enabled = true;
        interactBackground.GetComponent<Image>().enabled = true;
        interactFillBar.GetComponent<Image>().enabled = true;
    }
}