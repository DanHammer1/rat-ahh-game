using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class ProgressManager : NetworkBehaviour
{
    public TextMeshProUGUI timer;
    public TextMeshProUGUI objectivesUI;
    public TextMeshProUGUI playersUIList;
    public TextMeshProUGUI scoreList;

    public NetworkVariable<float> time = new NetworkVariable<float>(300);
    public List<Objective> objectives = new List<Objective>();

    private bool IsActive = false;
    private bool onActivateExecuted = false;

    public IEnumerator OnActivate()
    {
        if (onActivateExecuted) yield break;
        onActivateExecuted = true;

        GameObject timerGameObject = GameObject.FindWithTag("TimerUI");
        GameObject objectivesUIGameObject = GameObject.FindWithTag("ObjectivesUI");
        GameObject playersUIListGameObject = GameObject.FindWithTag("PlayerListUI");
        GameObject scoreListGameObject = GameObject.FindWithTag("Score");

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
        objectivesUI = objectivesUIGameObject.GetComponent<TextMeshProUGUI>();
        //playersUIList = playersUIListGameObject.GetComponent<TextMeshProUGUI>();
        scoreList = scoreListGameObject.GetComponent<TextMeshProUGUI>();

        objectives = new List<Objective>();

        if (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER) {
            objectives.Add(new CheeseObjective());
            objectives.Add(new DeliveryObjective());
        }

        IsActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer || !IsActive) return;

        //UpdatePlayerUIListClientRpc();
        UpdateScoreListClientRpc();

        time.Value -= Time.deltaTime;
        UpdateTimerClientRpc();

        UpdateObjectiveUIListClientRpc();
    }

    [ClientRpc]
    public void UpdateTimerClientRpc()
    {
        if (timer == null) return;
        timer.text = $"Time remaining: {(int)time.Value}";
    }

    [ClientRpc]
    public void UpdatePlayerUIListClientRpc()
    {
        if (playersUIList == null) return;

        string text = $"Hunters:\n";

        foreach (int i in GameManager.GetHunterIndexs())
        {
            string name = GameManager.Instance.clientNames[i].Value;
            text += $"{name}\n";
        }

        text += $"Hiders:\n";

        foreach (int i in GameManager.GetHiderIndexs())
        {
            string name = GameManager.Instance.clientNames[i].Value;
            text += $"{name}\n";
        }

        playersUIList.text = text;
    }

    [ClientRpc]
    public void UpdateScoreListClientRpc()
    {
        if (scoreList == null) return;

        string text = $"<b><u>Leaderboard</u></b>\n";

        foreach (int i in GameManager.GetHiderIndexs())
        {
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

    [ClientRpc]
    public void UpdateObjectiveUIListClientRpc()
    {
        if (objectivesUI == null) return;

        string text = "";

        foreach (Objective objective in objectives)
        {
            if (!objective.CheckConditionCleared()) text += objective.objectiveText + "\n";
        }

        objectivesUI.text = text;
    }
}
