using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using Unity.Cinemachine;
using UnityEditor;

public class Loading : NetworkBehaviour
{
    public CinemachineTargetGroup targetGroup;
    PlayerCamera playerCamera;
    void Start()
    {
        playerCamera = FindFirstObjectByType<PlayerCamera>();

        if (NetworkManager.Singleton.IsServer)
        {
            LoadGameScene();
        }
    }


    private void LoadGameScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(
            "Game",
            LoadSceneMode.Single
        );

        NetworkManager.Singleton.SceneManager.OnLoadComplete += (clientId, sceneName, loadMode) =>
        {
            if (NetworkManager.Singleton.IsServer) {
                GameManager.Instance.SpawnAllPlayers();
            }
        };
    }
}