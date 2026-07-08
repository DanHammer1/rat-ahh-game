using UnityEngine;
using TMPro;
using UnityEngine.UI;

public interface IInteractable
{
    public void Interact();

    public string GetInteractionPromptText();

    public float GetProgress();

    public void UpdateProgress();

    public void TryInteract() {
        if (CheckInteractionShouldTrigger()) Interact();
        TryUpdateProgress();
    }

    public static bool CheckPlayerFacingInteractableObject() {
        if (Player.localPlayer == null) return false;

        RaycastHit hit;
        if (Physics.Raycast(
            Player.localPlayer.viewPosition.transform.position,
            PlayerCamera.mainCamera.transform.forward,
            out hit,
            1f, LayerMask.GetMask("InteractableObject"))) {

            GameObject interactPrompt = GameObject.FindWithTag("InteractionPrompt");
            IInteractable implementationScript = hit.collider.gameObject.GetComponent<IInteractable>();
            string newInteractText = implementationScript.GetInteractionPromptText();
            interactPrompt.GetComponent<TextMeshProUGUI>().text = newInteractText;
            implementationScript.UpdateProgressBar(implementationScript.GetProgress());
            return true;
            }
        
        return false;
    }

    public bool CheckPlayerInRange() {
        if (Player.localPlayer == null) return false;

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
        return (CheckPlayerInRange() && Input.GetKey(KeyCode.E) && GetProgress() >= 1);
    }

    public void TryUpdateProgress() {
        if (CheckPlayerInRange() && Input.GetKey(KeyCode.E)) {
            UpdateProgress();
        } else {
            OnInteractingExit();
        }
    }

    public void OnInteractingExit() {}

    public void UpdateProgressBar(float progress) {
        GameObject.FindWithTag("ProgressFillBar").GetComponent<Image>().fillAmount = progress;
    }

    public static void TryDisplayInteractionText() {
        GameObject interactPrompt = GameObject.FindWithTag("InteractionPrompt");
        GameObject interactBackground = GameObject.FindWithTag("InteractionPromptBackground");
        GameObject interactFillBar = GameObject.FindWithTag("ProgressFillBar");

        if (interactPrompt == null) {
            Debug.LogError("No prompt found.");
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