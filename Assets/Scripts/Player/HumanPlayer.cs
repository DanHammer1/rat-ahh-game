using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.Animations.Rigging;
using System;

public class HumanPlayer : Player {
    public GameObject ratAbilityTarget;
    public NetworkVariable<bool> isBeingClung = new NetworkVariable<bool>(false);
    public NetworkVariable<float> ratAbilityHumanShakeMeter =
    new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    public NetworkVariable<int> slapCount = new NetworkVariable<int>();
    public NetworkVariable<bool> isCarryingItem = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isDizzy = new NetworkVariable<bool>(false);
    public RigBuilder rigBuilder;
    public float dizzyDuration;
    public int currentSlapCount;

    public static Action onHumanClung;

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetCarryingItemRpc(bool state) {
        isCarryingItem.Value = state;
    }

    [ServerRpc]
    public void SetIsDizzyServerRpc(bool state) {
        isDizzy.Value = state;
    }

    public void CheckJustGotClung(bool state) {
        if (state != isBeingClung.Value && state == true) onHumanClung?.Invoke();
    }

    void OnDrawGizmos() {
        if (viewPosition != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(viewPosition.transform.position, 0.01f);
        }

        if (ratAbilityTarget != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(ratAbilityTarget.transform.position, 0.01f);
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        if (IsServer) {
            // ratAbilityHumanShakeMeter.Value = 0f;
            slapCount.Value = 0;
        }

        rigBuilder = GetComponent<RigBuilder>();

        if (!IsOwner) return;

        PlayerCamera.instance.onFirstPersonEnter += EnableRigBuilderRpc;
        PlayerCamera.instance.onThirdPersonEnter += DisableRigBuilderRpc;

        GameObject.FindWithTag("AbilityParent").SetActive(false);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void DisableRigBuilderRpc() {
        rigBuilder.layers[0].active = false;
    }
    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void EnableRigBuilderRpc() {
        rigBuilder.layers[0].active = true;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdateRatAbilityShakeMeterRpc(float newValue) {
        ratAbilityHumanShakeMeter.Value = newValue;
    }

    protected override void Update() {
        if (!IsOwner) return;

        if (isBeingClung.Value) {
            movement.movementRecoveryMultiplier = Mathf.Exp(-0.1f * slapCount.Value);
            ratAbilityShakeUI?.SetActive(true);
            float mouseMovement = Mathf.Sqrt(Mathf.Pow(Input.GetAxis("Mouse X"), 2f) + Mathf.Pow(Input.GetAxis("Mouse Y"), 2));
            ratAbilityHumanShakeMeter.Value += Time.deltaTime;
            ratAbilityHumanShakeMeter.Value += mouseMovement / 100;
            // UpdateRatAbilityShakeMeterRpc(ratAbilityHumanShakeMeter.Value + mouseMovement / 100);
            if (ratAbilityHumanShakeMeter.Value > Constants.maxRatAbilityHumanShakeMeter) {
                // UpdateRatAbilityShakeMeterRpc(Constants.maxRatAbilityHumanShakeMeter);
                ratAbilityHumanShakeMeter.Value = Constants.maxRatAbilityHumanShakeMeter;
            }

            shakeProgressBarImage.fillAmount = Mathf.Clamp01(ratAbilityHumanShakeMeter.Value / Constants.maxRatAbilityHumanShakeMeter);
            Debug.Log(ratAbilityHumanShakeMeter.Value);
        } else if (isDizzy.Value) {
            ratAbilityShakeUI?.SetActive(false);
            // UpdateRatAbilityShakeMeterRpc(0);
            ratAbilityHumanShakeMeter.Value = 0;
        } else {
            ratAbilityShakeUI?.SetActive(false);
            // UpdateRatAbilityShakeMeterRpc(0);
            ratAbilityHumanShakeMeter.Value = 0;
            movement.isMovementLocked = false;
        }

        if (slapCount.Value > currentSlapCount) {
            CameraShakeManager.instance.CameraShake(impulseSource);
        }
        currentSlapCount = slapCount.Value;
    }

    public void UpdateDizzyDuration() {
        dizzyDuration = 1 + (slapCount.Value * 0.2f);
    }
}
