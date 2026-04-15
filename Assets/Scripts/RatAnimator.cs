using UnityEngine;
using Unity.Netcode;

public class RatAnimator : NetworkBehaviour
{
    Movement movement;
    RatPlayer ratPlayer;
    Animator animator;

    public override void OnNetworkSpawn()
    {
        movement = GetComponent<Movement>();
        ratPlayer = GetComponent<RatPlayer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        animator.SetFloat("Speed", movement.speed);

        animator.SetBool("isGrounded", movement.isGrounded);
        animator.SetBool("pressedSpace", movement.pressedSpace);

        animator.SetBool("isClinging", ratPlayer.isClinging);
        animator.SetBool("isSlapping", ratPlayer.isSlapping);

    }
}