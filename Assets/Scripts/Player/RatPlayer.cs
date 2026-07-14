using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using ParrelSync.NonCore;
using UnityEditor.Search;

public class RatPlayer : Player
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    
    protected override void Update()
    {
        base.Update();
    }
}