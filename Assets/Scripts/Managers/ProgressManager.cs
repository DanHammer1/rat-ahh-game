using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;
using System.Collections;
using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.Animations;
using UnityEngine.Events;
using Unity.Cinemachine;

public class ProgressManager : NetworkBehaviour {
    public TextMeshProUGUI timer;
    public TextMeshProUGUI objectiveUISlot1;
    public TextMeshProUGUI objectiveUISlot2;
    public TextMeshProUGUI objectiveUISlot3;
    public TextMeshProUGUI playersUIList;
    public TextMeshProUGUI scoreList;
    public TextMeshProUGUI returningToLobbyText;
    public CinemachineInputAxisController cinemachineCamera;

    public NetworkVariable<float> defaultTime = new NetworkVariable<float>(600);
    public NetworkVariable<float> time = new NetworkVariable<float>(10);
    public Timer returningToLobbyTimer;
    // public NetworkVariable<float> defaultReturningToLobbyTime = new NetworkVariable<float>(8);
    // public NetworkVariable<float> returningToLobbyTime = new NetworkVariable<float>(8);
    public List<Objective> objectives = new List<Objective>();
    public List<ObjectiveListSlot> objectiveListSlots = new();

    public bool IsActive = false;
    public bool onActivateExecuted = false;
    public bool isGameEnded;
    public bool movingToLobby = false;


    public static ProgressManager instance;

    void Awake() {
        instance = this;
    }
    public IEnumerator OnActivate() {
        if (onActivateExecuted) yield break;

        GameManager.gameState = GameManager.GameState.GAME;

        if (IsServer) time.Value = defaultTime.Value;

        onActivateExecuted = true;
        isGameEnded = false;
        movingToLobby = false;

        GameObject timerGameObject = GameObject.FindWithTag("TimerUI");
        GameObject objectivesUIGameObject = GameObject.FindWithTag("ObjectivesUI");
        //GameObject playersUIListGameObject = GameObject.FindWithTag("PlayerListUI");
        GameObject scoreListGameObject = GameObject.FindWithTag("Score");
        cinemachineCamera = GameObject.Find("CinemachineCamera").GetComponent<CinemachineInputAxisController>();

        while (timerGameObject == null ||
            objectivesUIGameObject == null ||
            scoreListGameObject == null ||
            CheeseSpawner.instance == null) {

            timerGameObject = GameObject.FindWithTag("TimerUI");
            objectivesUIGameObject = GameObject.FindWithTag("ObjectivesUI");
            //playersUIListGameObject = GameObject.FindWithTag("PlayerListUI");
            scoreListGameObject = GameObject.FindWithTag("Score");

            yield return null;
        }

        timer = timerGameObject.GetComponent<TextMeshProUGUI>();
        objectiveListSlots.Add(new ObjectiveListSlot {
            text = objectivesUIGameObject.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>()
        });
        objectiveListSlots.Add(new ObjectiveListSlot {
            text = objectivesUIGameObject.transform.GetChild(1).Find("Text").GetComponent<TextMeshProUGUI>()
        });
        objectiveListSlots.Add(new ObjectiveListSlot {
            text = objectivesUIGameObject.transform.GetChild(2).Find("Text").GetComponent<TextMeshProUGUI>()
        });

        //playersUIList = playersUIListGameObject.GetComponent<TextMeshProUGUI>();
        scoreList = scoreListGameObject.GetComponent<TextMeshProUGUI>();

        objectives = new List<Objective>();

        if (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER) { }

        IsActive = true;

    }
    public void OnDeactivate() {

        GameManager.gameState = GameManager.GameState.LOBBY;

        onActivateExecuted = false;

        IsActive = false;
        this.enabled = false;

    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetMatchLengthRpc(float newTime) {
        defaultTime.Value = newTime;
    }

    // Update is called once per frame
    void Update() {
        if (!IsServer || !IsActive || !NetworkManager.Singleton) return;

        //UpdatePlayerUIListClientRpc();
        UpdateScoreListClientRpc();

        if (!isGameEnded) {
            time.Value -= Time.deltaTime;
            UpdateTimerClientRpc();
        }

        if (isGameEnded && !movingToLobby) {
            UpdateReturnToLobbyTimerClientRpc();
        }

        UpdateObjectiveUIListClientRpc();
    }

    [ClientRpc]
    public void UpdateTimerClientRpc() {
        if (timer == null) {
            // Debug.Log("timer is null");
            return;
        }
        timer.text = $"Time remaining: {(int)time.Value}";

        if (time.Value < 0 && IsServer && !isGameEnded) {
            OnGameEnd();
        }
    }

    [ClientRpc]
    public void UpdateReturnToLobbyTimerClientRpc() {
        if (returningToLobbyText == null) {
            Debug.Log("returnToLobbyText is null");
            return;
        }
        returningToLobbyText.text = $"(Returning to lobby in {(int)returningToLobbyTimer.GetTimeRemaining()}...)";
    }

    void OnGameEnd() {
        isGameEnded = true;
        time.Value = 0;
        CreateResults();
        DisableGameplay();
        returningToLobbyTimer = Timer.CreateTimer(8f, Timer.OnFinish.DESTROY, // todo move time to constants.cs
            () => {
                movingToLobby = true;
                GameManager.Instance.DespawnObjects();
                NetworkManager.Singleton.SceneManager.LoadScene(
                    "LoadingScreen",
                LoadSceneMode.Single);
            }).GetComponent<Timer>();
    }

    void CreateResults() {
        Assets.instance.endGameResults?.SetActive(true);
        returningToLobbyText = GameObject.FindWithTag("ReturningToLobbyText").GetComponent<TextMeshProUGUI>();
        foreach (var (clientId, rank) in OrderByScore()) {
            if (GameManager.GetRole(clientId) == GameManager.PlayerRole.HUNTER) continue;
            GameObject playerResult = Instantiate(Assets.instance.playerResult, GameObject.Find("PlayerRankings").transform);

            // set colour based on rank
            UnityEngine.UI.Image image = playerResult.GetComponent<UnityEngine.UI.Image>();
            if (rank == 1) image.color = new Color(1.0f, 0.84f, 0.0f); // gold
            else if (rank == 2) image.color = new Color(1.0f, 0.84f, 0.0f); // silver
            else if (rank == 3) image.color = new Color(1.0f, 0.84f, 0.0f); // bronze
            else image.color = new Color( // random brown
                UnityEngine.Random.Range(0.25f, 0.45f), // R
                UnityEngine.Random.Range(0.10f, 0.25f), // G
                UnityEngine.Random.Range(0.03f, 0.12f)  // B
            );

            // set rank, username & score
            playerResult.transform.Find("RankingText").GetComponent<TextMeshProUGUI>().text = rank.ToString();
            playerResult.transform.Find("UsernameText").GetComponent<TextMeshProUGUI>().text = GameManager.GetName(clientId).ToString();
            playerResult.transform.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = GetScore(clientId).ToString();
        }
    }

    void DisableGameplay() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Player.localPlayer.GetComponent<Movement>().isMovementLocked = true;
        cinemachineCamera.enabled = false;
    }

    Dictionary<ulong, int> OrderByScore() {
        Dictionary<ulong, int> idsByScore = new Dictionary<ulong, int>();
        List<ulong> sortedIds = GameManager.GetHiderIds();
        sortedIds.Sort((a, b) => GetScore(b).CompareTo(GetScore(a)));
        int i = 0;
        while (i < sortedIds.Count) {
            idsByScore[sortedIds[i]] = i + 1;
            int currentRank = i + 1;
            while (i + 1 < sortedIds.Count && (GetScore(sortedIds[i]) == GetScore(sortedIds[i + 1]))) {
                idsByScore[sortedIds[i + 1]] = currentRank;
                i++;
            }
            i++;
        }
        return idsByScore;
    }

    IEnumerator WaitSeconds(int seconds) {
        yield return new WaitForSeconds(seconds);
    }

    [ClientRpc]
    public void UpdatePlayerUIListClientRpc() {
        if (playersUIList == null) return;

        string text = $"Hunters:\n";

        foreach (int i in GameManager.GetHunterIndexs()) {
            string name = GameManager.Instance.clientNames[i].Value;
            text += $"{name}\n";
        }

        text += $"Hiders:\n";

        foreach (int i in GameManager.GetHiderIndexs()) {
            string name = GameManager.Instance.clientNames[i].Value;
            text += $"{name}\n";
        }

        playersUIList.text = text;
    }

    [ClientRpc]
    public void UpdateScoreListClientRpc() {
        if (scoreList == null) return;

        string text = $"<b><u>Leaderboard</u></b>\n";

        foreach (int i in GameManager.GetHiderIndexs()) {
            Player[] players = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None);

            foreach (Player player in players) {
                if (GameManager.Instance.clientIds[i] == player.clientId.Value) {
                    string name = GameManager.Instance.clientNames[i].Value;
                    text += $"{name}: {player.score.Value}\n";
                    break;
                }
            }
        }

        scoreList.text = text;
    }

    public int GetScore(ulong clientId) {
        Player[] players = GameObject.FindObjectsByType<Player>(FindObjectsSortMode.None);
        Player player = Array.Find(players, p => p.clientId.Value == clientId);
        return player.score.Value;
    }

    [ClientRpc]
    public void UpdateObjectiveUIListClientRpc() {
        foreach (var slot in objectiveListSlots) {
            if (slot == null) {
                Debug.Log("objective UI slot is null");
                return;
            }
        }

        List<Objective> objectivesToRemove = new List<Objective>();

        foreach (Objective objective in objectives) {
            if (objective.CheckConditionCleared()) {
                objectivesToRemove.Add(objective);
                StartCoroutine(ClearObjectiveText(objective));
            }
        }

        foreach (Objective objective in objectivesToRemove) {
            objective.onConditionCleared?.Invoke();
        }

        foreach (Objective objective in objectives) {
            AssignObjectiveText(objective);
        }
    }

    public void AssignObjectiveText(Objective objective) {
        foreach (var slot in objectiveListSlots) {
            if (slot.currentObjective == objective) {
                return;
            }
        }

        foreach (var slot in objectiveListSlots) {
            if (slot.currentObjective == null) {
                slot.currentObjective = objective;
                slot.text.text = objective.objectiveText;
                slot.text.transform.localScale = Vector3.one;
                UnityEngine.UI.Image checkbox = slot.text.transform.parent.Find("Checkbox").GetComponent<UnityEngine.UI.Image>();
                checkbox.color = new Color(checkbox.color.r, checkbox.color.g, checkbox.color.b, 1);
                return;
            }
        }

        Debug.Log("error - no objective slots available");
    }

    public IEnumerator ClearObjectiveText(Objective objective) {
        foreach (var slot in objectiveListSlots) {
            if (slot.currentObjective == objective) {
                slot.currentObjective = null;

                Transform ratStamp = slot.text.transform.parent.Find("Checkbox/RatStamp");
                GameObject ratStampObject = ratStamp.gameObject;
                UnityEngine.UI.Image ratStampImage = ratStampObject.GetComponent<UnityEngine.UI.Image>();
                UnityEngine.UI.Image checkbox = slot.text.transform.parent.Find("Checkbox").GetComponent<UnityEngine.UI.Image>();

                ratStampObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(UnityEngine.Random.Range(-6, 6), UnityEngine.Random.Range(-6, 6));
                ratStampObject.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-30, 30));
                ratStampObject.transform.localScale = Vector3.one * 30;
                ratStampImage.color = new Color(ratStampImage.color.r, ratStampImage.color.g, ratStampImage.color.b, 0);
                ratStampObject.SetActive(true);
                ratStampObject.LeanScale(Vector3.one, 0.3f).setEase(LeanTweenType.easeInQuad);
                LeanTween.value(ratStampObject, 0, 1f, 0.3f).setEase(LeanTweenType.easeInQuad).setOnUpdate((float alpha) => {
                    Color c = ratStampImage.color;
                    c.a = alpha;
                    ratStampImage.color = c;
                });

                yield return new WaitForSeconds(2);
                slot.text.gameObject.LeanScale(new Vector3(0, 0, 0), 0.5f).setEaseInBack();
                ratStampObject.LeanScale(new Vector3(0, 0, 0), 0.5f).setEaseInBack();
                checkbox.color = new Color(checkbox.color.r, checkbox.color.g, checkbox.color.b, 0.3f);
            }
        }
    }
}
