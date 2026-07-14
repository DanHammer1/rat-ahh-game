using UnityEngine;
using Unity.Netcode;
using UnityEngine.Animations.Rigging;

public class PlayerAnimator : NetworkBehaviour
{
    public static PlayerAnimator instance;
    Movement movement;
    Animator animator;
    bool isTwerking;
    bool isARAT;

    public override void OnNetworkSpawn()
    {
        movement = GetComponent<Movement>();
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationName, string animationBool, float length, int layer = 0)
    {
        PlayAnimationServerRpc(animationName, animationBool, length, layer);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void PlayAnimationServerRpc(string animationName, string animationBool, float length, int layer)
    {
        PlayAnimationClientRpc(animationName, animationBool, length, layer);
    }

    [ClientRpc]
    public void PlayAnimationClientRpc(string animationName, string animationBool, float length, int layer)
    {
        animator.SetBool(animationBool, true);
        animator.CrossFade(animationName, length, layer);
        animator.SetBool(animationBool, false);
    }

    void Update()
    {
        if (!IsOwner) return;

        if (instance == null)
        {
            instance = this;
        }

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
