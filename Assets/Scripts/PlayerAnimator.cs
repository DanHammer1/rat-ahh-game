using UnityEngine;
using Unity.Netcode;

public class PlayerAnimator : NetworkBehaviour
{
    Movement movement;
    Animator animator;
    bool isTwerking;
    bool isARAT;

    public override void OnNetworkSpawn()
    {
        movement = GetComponent<Movement>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner) return;
        animator.SetFloat("Speed", movement.speed);

        animator.SetFloat("Forward", movement.moveForward);
        animator.SetFloat("Right", movement.moveHorizontal);

        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetBool("isTwerking", true);
            animator.CrossFade("Twerk", 0.3f);
            animator.SetBool("isTwerking", false);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetBool("isARAT", true);
            animator.CrossFade("ARAT", 0.3f);
            animator.SetBool("isARAT", false);
        }
    }
}
