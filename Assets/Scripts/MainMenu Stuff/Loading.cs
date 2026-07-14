using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;

public class Loading : NetworkBehaviour
{
    public CinemachineTargetGroup targetGroup;
    PlayerCamera playerCamera;
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        playerCamera = FindFirstObjectByType<PlayerCamera>();

        if (NetworkManager.Singleton.IsServer)
        {
            LoadGameScene();
        }
    }

    private void LoadGameScene()
    {
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            "Game",
            LoadSceneMode.Single
        );
    }

    private void OnLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut) {
        
        if (!NetworkManager.Singleton.IsServer)
            return;

        StartCoroutine(GameManager.Instance.SpawnAllPlayers());
        GameManager.Instance.OnGameStartClientRpc();

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }
}