using UnityEngine;
using Unity.Netcode;

public class RatAnimator : NetworkBehaviour
{
    Movement movement;
    Animator animator;

    public override void OnNetworkSpawn()
    {
        movement = GetComponent<Movement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        animator.SetFloat("Speed", movement.speed);

        animator.SetBool("isGrounded", movement.isGrounded);
        animator.SetBool("pressedSpace", movement.pressedSpace);
        Debug.Log(movement.isGrounded);
    }
}