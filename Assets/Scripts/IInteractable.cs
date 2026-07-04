using UnityEngine;
using TMPro;

public interface IInteractable
{
    public void Interact();

    public string GetInteractionPromptText();

    public void TryInteract() {
        if (CheckInteractionShouldTrigger()) Interact();
    }

    public static bool CheckPlayerFacingInteractableObject() {
        RaycastHit hit;
        if (Physics.Raycast(
            Player.localPlayer.viewPosition.transform.position,
            PlayerCamera.mainCamera.transform.forward,
            out hit,
            1f, LayerMask.GetMask("InteractableObject"))) {

            GameObject interactPrompt = GameObject.FindWithTag("InteractionPrompt");
            string newInteractText = hit.collider.gameObject.GetComponent<IInteractable>().GetInteractionPromptText();
            interactPrompt.GetComponent<TextMeshProUGUI>().text = newInteractText;
            return true;
            }
        
        return false;
    }

    public bool CheckPlayerInRange() {
        RaycastHit hit;
        if (Player.localPlayer.viewPosition == null) {
            Debug.LogError("viewPosition is gone.");
            return false;
        }
        else if (PlayerCamera.mainCamera == null) {
            Debug.LogError("mainCamera is gone.");
        }
        if (Physics.Raycast(
            Player.localPlayer.viewPosition.transform.position,
            PlayerCamera.mainCamera.transform.forward,
            out hit,
            1f, LayerMask.GetMask("InteractableObject"))) {
                if (hit.collider.gameObject == null) return false;

                GameObject hitObject = hit.collider.gameObject;
                if (hitObject.GetComponent<IInteractable>() == this) {
                    return true;
                }
            }
        
        return false;
    }

    public bool CheckInteractionShouldTrigger() {
        return (CheckPlayerInRange() && Input.GetKeyDown(KeyCode.E));
    }

    public static void TryDisplayInteractionText() {
        GameObject interactPrompt = GameObject.FindWithTag("InteractionPrompt");

        if (interactPrompt == null) {
            Debug.LogError("No prompt found.");
            return;
        }

        if (!CheckPlayerFacingInteractableObject()) {
            interactPrompt.GetComponent<TextMeshProUGUI>().enabled = false;
            return;
        }

        interactPrompt.GetComponent<TextMeshProUGUI>().enabled = true;
    }
}