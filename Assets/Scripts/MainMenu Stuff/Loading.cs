using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEditor;
using Unity.VisualScripting;

public class Loading : MonoBehaviour {
    public CinemachineTargetGroup targetGroup;
    PlayerCamera playerCamera;
    public static Loading instance;
    void Awake() {
        if (instance != null) {
            Destroy(this.gameObject);
            return;
        } else instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() {

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
                GameManager.gameState = GameManager.GameState.LOBBY;
                sceneToLoad = "Lobby";
                break;
            case GameManager.GameState.LOBBY:
                GameManager.gameState = GameManager.GameState.GAME;
                sceneToLoad = "Game";
                break;
            case GameManager.GameState.GAME:
                GameManager.gameState = GameManager.GameState.LOBBY;
                sceneToLoad = "Lobby";
                break;
            default:
                GameManager.gameState = GameManager.GameState.LOBBY;
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

        StartCoroutine(FinishLoading());
    }

    private IEnumerator FinishLoading() {
        yield return StartCoroutine(GameManager.Instance.SpawnAllPlayers());

        if (GameManager.gameState == GameManager.GameState.GAME) {
            GameManager.Instance.OnGameStartClientRpc();
        } else {
            GameManager.Instance.OnLobbyStartClientRpc();
        }

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;

        Destroy(gameObject);
    }
}