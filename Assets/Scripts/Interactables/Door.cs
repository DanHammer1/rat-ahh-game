using UnityEngine;
using Unity.Netcode;
using System;

public class Door : NetworkBehaviour, IInteractable
{
    public Collider doorCollider;
    private Animator animator;

    public enum State
    {
        OPEN,
        CLOSED
    }

    public State doorState = State.CLOSED;

    public Action onDoorClosed;
    public Action onDoorOpened;

    public String GetInteractionPromptText()
    {
        switch (doorState)
        {
            case State.OPEN:
                return "Press E to close Door";
            case State.CLOSED:
                return "Press E to open Door";
            default:
                return "Door has no state.";
        }
    }
    public void Interact()
    {
        InteractDoorRpc();
    }

    public void SwitchDoorState()
    {
        if (doorState == State.OPEN)
        {
            doorState = State.CLOSED;
            onDoorClosed?.Invoke();
        }
        else
        {
            doorState = State.OPEN;
            onDoorOpened?.Invoke();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void InteractDoorRpc()
    {
        InteractDoorClientRpc();
    }

    [ClientRpc]
    public void InteractDoorClientRpc()
    {
        if (doorState == State.OPEN)
        {
            animator.CrossFade("CLOSE", 0.15f);
        }
        else
        {
            animator.CrossFade("OPEN", 0.15f);
        }
        SwitchDoorState();
    }

    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        onDoorClosed += () =>
        {
            GetComponent<BoxCollider>().isTrigger = false;
        };
        onDoorOpened += () =>
        {
            GetComponent<BoxCollider>().isTrigger = true;
        };
    }

    void Update()
    {
        ((IInteractable)this).TryInteract();
    }
}