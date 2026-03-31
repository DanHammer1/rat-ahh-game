using UnityEngine;
using Unity.Netcode;

public class PlayerAnimator : NetworkBehaviour
{
    Movement movement;
    Animator animator;

    public override void OnNetworkSpawn() {
        movement = GetComponent<Movement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetFloat("Speed", movement.speed);

        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.CrossFade("Twerk", 0.3f);
        }
    }
}
