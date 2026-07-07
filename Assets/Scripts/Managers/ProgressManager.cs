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

    public IEnumerator OnActivate()
    {
        objectives = new List<Objective>();
        objectives.Add(new CheeseObjective());
        objectives.Add(new DeliveryObjective());

        GameObject timerGameObject = GameObject.FindWithTag("TimerUI");
        GameObject objectivesUIGameObject = GameObject.FindWithTag("ObjectivesUI");
        GameObject playersUIListGameObject = GameObject.FindWithTag("PlayerListUI");

        while (timerGameObject == null || objectivesUIGameObject == null || playersUIListGameObject == null)
        {
            yield return null;
        }

        timer = timerGameObject.GetComponent<TextMeshProUGUI>();
        objectivesUI = objectivesUIGameObject.GetComponent<TextMeshProUGUI>();
        playersUIList = playersUIListGameObject.GetComponent<TextMeshProUGUI>();

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
