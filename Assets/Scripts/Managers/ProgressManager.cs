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

public class ProgressManager : NetworkBehaviour
{
    public float defaultTime;
    public TextMeshProUGUI timer;
    public TextMeshProUGUI objectiveUISlot1;
    public TextMeshProUGUI objectiveUISlot2;
    public TextMeshProUGUI objectiveUISlot3;
    public TextMeshProUGUI playersUIList;
    public TextMeshProUGUI scoreList;

    public NetworkVariable<float> time = new NetworkVariable<float>(10);
    public List<Objective> objectives = new List<Objective>();
    public List<ObjectiveListSlot> objectiveListSlots = new();

    public bool IsActive = false;
    public bool onActivateExecuted = false;

    public static ProgressManager instance;

    public IEnumerator OnActivate()
    {
        instance = this;

        if (onActivateExecuted) yield break;

        if (IsServer) time.Value = defaultTime;

        onActivateExecuted = true;

        GameObject timerGameObject = GameObject.FindWithTag("TimerUI");
        GameObject objectivesUIGameObject = GameObject.FindWithTag("ObjectivesUI");
        //GameObject playersUIListGameObject = GameObject.FindWithTag("PlayerListUI");
        GameObject scoreListGameObject = GameObject.FindWithTag("Score");

        while (timerGameObject == null ||
            objectivesUIGameObject == null ||
            scoreListGameObject == null ||
            CheeseSpawner.instance == null)
        {

            timerGameObject = GameObject.FindWithTag("TimerUI");
            objectivesUIGameObject = GameObject.FindWithTag("ObjectivesUI");
            //playersUIListGameObject = GameObject.FindWithTag("PlayerListUI");
            scoreListGameObject = GameObject.FindWithTag("Score");

            yield return null;
        }

        timer = timerGameObject.GetComponent<TextMeshProUGUI>();
        objectiveListSlots.Add(new ObjectiveListSlot
        {
            text = objectivesUIGameObject.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>()
        });
        objectiveListSlots.Add(new ObjectiveListSlot
        {
            text = objectivesUIGameObject.transform.GetChild(1).Find("Text").GetComponent<TextMeshProUGUI>()
        });
        objectiveListSlots.Add(new ObjectiveListSlot
        {
            text = objectivesUIGameObject.transform.GetChild(2).Find("Text").GetComponent<TextMeshProUGUI>()
        });

        //playersUIList = playersUIListGameObject.GetComponent<TextMeshProUGUI>();
        scoreList = scoreListGameObject.GetComponent<TextMeshProUGUI>();

        objectives = new List<Objective>();

        if (GameManager.GetLocalRole() == GameManager.PlayerRole.HIDER) { }

        IsActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer || !IsActive || !NetworkManager.Singleton) return;

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

        if (time.Value < 0 && IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
            "MainMenu",
            LoadSceneMode.Single);
        }
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

            foreach (Player player in players)
            {
                if (GameManager.Instance.clientIds[i] == player.clientId.Value)
                {
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
        foreach (var slot in objectiveListSlots)
        {
            if (slot == null)
            {
                Debug.Log("objective UI slot is null");
                return;
            }
        }

        List<Objective> objectivesToRemove = new List<Objective>();

        foreach (Objective objective in objectives)
        {
            if (objective.CheckConditionCleared())
            {
                objectivesToRemove.Add(objective);
                StartCoroutine(ClearObjectiveText(objective));
            }
        }

        foreach (Objective objective in objectivesToRemove)
        {
            objective.onConditionCleared?.Invoke();
        }

        foreach (Objective objective in objectives)
        {
            AssignObjectiveText(objective);
        }
    }

    public void AssignObjectiveText(Objective objective)
    {
        foreach (var slot in objectiveListSlots)
        {
            if (slot.currentObjective == objective)
            {
                return;
            }
        }

        foreach (var slot in objectiveListSlots)
        {
            if (slot.currentObjective == null)
            {
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

    public IEnumerator ClearObjectiveText(Objective objective)
    {
        foreach (var slot in objectiveListSlots)
        {
            if (slot.currentObjective == objective)
            {
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
                LeanTween.value(ratStampObject, 0, 1f, 0.3f).setEase(LeanTweenType.easeInQuad).setOnUpdate((float alpha) =>
                {
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
