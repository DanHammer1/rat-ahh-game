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
}
