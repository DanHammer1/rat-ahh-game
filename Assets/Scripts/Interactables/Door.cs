using UnityEngine;
using Unity.Netcode;
using System;

public class Door : NetworkBehaviour, IInteractable
{
    public Collider doorCollider;

    public enum State {
        OPEN,
        CLOSED
    }

    public State doorState = State.CLOSED;

    public Action onDoorClosed;
    public Action onDoorOpened;

    public void Interact() {
        InteractDoorRpc();
    }

    public void SwitchDoorState() {
        if (doorState == State.OPEN) {
            doorState = State.CLOSED;
            onDoorClosed?.Invoke();
        } else {
            doorState = State.OPEN;
            onDoorOpened?.Invoke();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void InteractDoorRpc() {
        InteractDoorClientRpc();
    }

    [ClientRpc]
    public void InteractDoorClientRpc() {
        if (doorState == State.OPEN) {
            GetComponent<Animator>().CrossFade("CLOSE", 0);
            Debug.Log("Closing.");
        } else {
            GetComponent<Animator>().CrossFade("OPEN", 0);
            Debug.Log("Opening");
        }
        SwitchDoorState();
    }

    void Start() {
        doorCollider = this.GetComponent<BoxCollider>();
        onDoorClosed += () => {
            GetComponent<Collider>().enabled = true;
        };
        onDoorOpened += () => {
            GetComponent<Collider>().enabled = false;
        };
    }

    void Update() {
        ((IInteractable)this).TryInteract();
    }
}
