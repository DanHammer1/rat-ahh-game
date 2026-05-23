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

    public NetworkVariable<float> time = new NetworkVariable<float>(300);
    public List<Objective> objectives = new List<Objective>();

    private bool IsActive = false;
    
    public void OnActivate() {
        objectives = new List<Objective>();

        timer = GameObject.FindWithTag("TimerUI").GetComponent<TextMeshProUGUI>();
        objectivesUI = GameObject.FindWithTag("ObjectivesUI").GetComponent<TextMeshProUGUI>();
        playersUIList = GameObject.FindWithTag("PlayerListUI").GetComponent<TextMeshProUGUI>();

        objectives.Add(new CheeseObjective());

        IsActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer || !IsActive) return;

        UpdatePlayerUIListClientRpc();
        
        time.Value -= Time.deltaTime;
        UpdateTimerClientRpc();

        UpdateObjectiveUIListClientRpc();
    }

    [ClientRpc]
    public void UpdateTimerClientRpc() {
        if (timer == null) return;
        timer.text = $"Time remaining: {(int)time.Value}";
    }

    [ClientRpc]
    public void UpdatePlayerUIListClientRpc() {
        if (playersUIList == null) return;

        string text = $"Hunters:\n";

        foreach (int i in GameManager.Instance.GetHunterIndexs()) {
            string name = GameManager.Instance.clientNames[i].Value;
            text += $"{name}\n";
        }
        
        text += $"Hiders:\n";

        foreach (int i in GameManager.Instance.GetHiderIndexs()) {
            string name = GameManager.Instance.clientNames[i].Value;
            text += $"{name}\n";
        }

        playersUIList.text = text;
    }

    [ClientRpc]
    public void UpdateObjectiveUIListClientRpc() {
        if (objectivesUI == null) return;

        string text = $"Objectives:\n";

        foreach (Objective objective in objectives) {
            if (!objective.CheckConditionCleared()) text += objective.objectiveText;
        }

        objectivesUI.text = text;
    }
}
