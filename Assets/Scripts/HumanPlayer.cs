using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.Animations.Rigging;

public class HumanPlayer : Player
{
    public GameObject viewPosition;
    public GameObject ratAbilityTarget;
    public NetworkVariable<bool> isBeingClung = new NetworkVariable<bool>(false);
    public NetworkVariable<float> ratAbilityHumanShakeMeter = new NetworkVariable<float>();
    public NetworkVariable<int> slapCount = new NetworkVariable<int>();
    public NetworkVariable<bool> isDizzy = new NetworkVariable<bool>(false);
    public RigBuilder rigBuilder;
    public float dizzyDuration;
    public int currentSlapCount;


    void OnDrawGizmos()
    {
        if (viewPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(viewPosition.transform.position, 0.01f);
        }

        if (ratAbilityTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(ratAbilityTarget.transform.position, 0.01f);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            ratAbilityHumanShakeMeter.Value = 0f;
            slapCount.Value = 0;
        }

        rigBuilder = GetComponent<RigBuilder>();

        if (!IsOwner) return;

        PlayerCamera.instance.onFirstPersonEnter += EnableRigBuilder;
        PlayerCamera.instance.onThirdPersonEnter += DisableRigBuilder;
    }

    public void DisableRigBuilder()
    {
        rigBuilder.layers[0].active = false;
        Debug.Log("Disabled.");
    }
    public void EnableRigBuilder()
    {
        rigBuilder.layers[0].active = true;
        Debug.Log("Enabled.");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateRatAbilityShakeMeterRpc(float newValue) {
        ratAbilityHumanShakeMeter.Value = newValue;
    }

    protected override void Update()
    {
        if (!IsOwner) return;
        if (isBeingClung.Value)
        {
            movement.movementRecoveryMultiplier = Mathf.Exp(-0.1f * slapCount.Value);
            ratAbilityShakeUI.SetActive(true);
            float mouseMovement = Mathf.Sqrt(Mathf.Pow(Input.GetAxis("Mouse X"), 2f) + Mathf.Pow(Input.GetAxis("Mouse Y"), 2));
            UpdateRatAbilityShakeMeterRpc(ratAbilityHumanShakeMeter.Value + mouseMovement / 100);
            if (ratAbilityHumanShakeMeter.Value > Constants.maxRatAbilityHumanShakeMeter)
            {
                UpdateRatAbilityShakeMeterRpc(Constants.maxRatAbilityHumanShakeMeter);
            }

            shakeProgressBarImage.fillAmount = Mathf.Clamp01(ratAbilityHumanShakeMeter.Value / Constants.maxRatAbilityHumanShakeMeter);
            Debug.Log(ratAbilityHumanShakeMeter.Value);
        }
        else if (isDizzy.Value)
        {
            ratAbilityShakeUI.SetActive(false);
            UpdateRatAbilityShakeMeterRpc(0);
        }
        else
        {
            ratAbilityShakeUI.SetActive(false);
            UpdateRatAbilityShakeMeterRpc(0);
            movement.isMovementLocked = false;
        }

        if (slapCount.Value > currentSlapCount)
        {
            CameraShakeManager.instance.CameraShake(impulseSource);
        }
        currentSlapCount = slapCount.Value;
    }

    public void UpdateDizzyDuration()
    {
        dizzyDuration = 1 + (slapCount.Value * 0.2f);
    }


}
