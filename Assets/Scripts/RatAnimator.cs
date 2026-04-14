using UnityEngine;
using Unity.Netcode;

public class RatAnimator : NetworkBehaviour
{
    Movement movement;
    Player player;
    Animator animator;

    public override void OnNetworkSpawn()
    {
        movement = GetComponent<Movement>();
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        animator.SetFloat("Speed", movement.speed);

        animator.SetBool("isGrounded", movement.isGrounded);
        animator.SetBool("pressedSpace", movement.pressedSpace);

        animator.SetBool("isClinging", player.isClinging);
        animator.SetBool("isSlapping", player.isSlapping);

    }
}