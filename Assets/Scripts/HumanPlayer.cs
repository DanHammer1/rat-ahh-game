using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;

public class HumanPlayer : Player
{
    public GameObject viewPosition;
    public GameObject ratAbilityTarget;
    public NetworkVariable<bool> isBeingClung = new NetworkVariable<bool>(false);
    public NetworkVariable<float> ratAbilityHumanShakeMeter = new NetworkVariable<float>();
    public NetworkVariable<int> slapCount = new NetworkVariable<int>();


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
        }
        if (!IsOwner) return;
        slapCount.Value = 0;
    }

    protected override void Update()
    {
        base.Update();
        if (!IsOwner) return;
        if (isBeingClung.Value)
        {
            movement.isMovementLocked = true;
            ratAbilityShakeUI.SetActive(true);
            float mouseMovement = Mathf.Sqrt(Mathf.Pow(Input.GetAxis("Mouse X"), 2f) + Mathf.Pow(Input.GetAxis("Mouse Y"), 2));
            ratAbilityHumanShakeMeter.Value += mouseMovement / 100;
            if (ratAbilityHumanShakeMeter.Value > Constants.maxRatAbilityHumanShakeMeter)
            {
                ratAbilityHumanShakeMeter.Value = Constants.maxRatAbilityHumanShakeMeter;
            }

            shakeProgressBarImage.fillAmount = Mathf.Clamp01(ratAbilityHumanShakeMeter.Value / Constants.maxRatAbilityHumanShakeMeter);
        }
        else
        {
            ratAbilityShakeUI.SetActive(false);
            ratAbilityHumanShakeMeter.Value = 0f;
            movement.isMovementLocked = false;
        }

    }
}
