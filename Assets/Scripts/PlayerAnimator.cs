using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations.Rigging;

public class PlayerAnimator : NetworkBehaviour
{
    Movement movement;
    static Animator animator;
    bool isTwerking;
    bool isARAT;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        movement = GetComponent<Movement>();
        animator = GetComponent<Animator>();
    }

    public static void PlayAnimation(string animationName, string animationBool, float length)
    {
        animator.SetBool(animationBool, true);
        animator.CrossFade(animationName, length);
        animator.SetBool(animationBool, false);
    }

    void Update()
    {
        if (!IsOwner) return;
        animator.SetFloat("Speed", movement.speed);

        animator.SetFloat("Forward", movement.moveForward);
        animator.SetFloat("Right", movement.moveHorizontal);

        if (Input.GetKeyDown(KeyCode.C))
        {
            PlayAnimation("Twerk", "isTwerking", 0.3f);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayAnimation("ARAT", "isARAT", 0.3f);
        }
    }
}
