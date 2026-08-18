using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;

public class Loading : NetworkBehaviour {
    public CinemachineTargetGroup targetGroup;
    PlayerCamera playerCamera;
    void Start() {
        DontDestroyOnLoad(gameObject);

        playerCamera = FindFirstObjectByType<PlayerCamera>();

        if (NetworkManager.Singleton.IsServer) {
            LoadGameScene();
        }
    }

    private void LoadGameScene() {
        if (NetworkManager.Singleton.IsServer) {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
        }
        string sceneToLoad;
        switch (GameManager.gameState) {
            case GameManager.GameState.MAINMENU:
                sceneToLoad = "Lobby";
                break;
            case GameManager.GameState.LOBBY:
                sceneToLoad = "Game";
                break;
            case GameManager.GameState.GAME:
                sceneToLoad = "Lobby";
                break;
            default:
                sceneToLoad = "Lobby";
                break;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(
            $"{sceneToLoad}",
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
        if (GameManager.gameState == GameManager.GameState.LOBBY) {
            GameManager.Instance.OnGameStartClientRpc();
        } else {
            GameManager.Instance.OnLobbyStartClientRpc();
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }
}