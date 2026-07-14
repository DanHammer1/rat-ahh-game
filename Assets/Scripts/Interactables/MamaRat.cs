using UnityEngine;
using Unity.Netcode;
using System;

public class MamaRat : NetworkBehaviour, IInteractable
{
    private Animator animator;

    private float interactionProgress = 0;
    private float interactionCompletionTime = 1f;
    private bool interactable = true;

    public String GetInteractionPromptText()
    {
        return (GameManager.GetLocalRole() != GameManager.PlayerRole.HIDER) ? 
            "Only rats can speak rat language." : "Hold E to talk to mama rat.";

    }
    public void Interact()
    {
        interactionProgress = 0;
    }

    public void OnInteractingExit() {
        interactionProgress = 0;
    }

    public void UpdateProgress() {
        if (!interactable) return;

        if (interactionCompletionTime == 0) interactionProgress = 1;
        else if (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER) 
            interactionProgress += Time.deltaTime / interactionCompletionTime;
    }

    public float GetProgress() {
        return interactionProgress;
    }

    void Update()
    {
        ((IInteractable)this).TryInteract();
    }
}