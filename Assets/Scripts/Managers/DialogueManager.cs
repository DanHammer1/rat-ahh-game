using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour {
    private TextMeshProUGUI dialogueDisplay;
    private GameObject dialogueUI;
    private ObjectiveSpawner objectiveSpawner;
    private MamaRat mamaRatScript;

    public Action onDialogueActivate;
    public Action onDialogueEnd;

    public static DialogueManager instance { get; private set; }

    public void CreateDialogue(string text) {
        dialogueUI.SetActive(true);
        dialogueDisplay.text = text;
        onDialogueActivate?.Invoke();
    }

    void Start() {
        dialogueUI = GameObject.FindWithTag("DialogueUI");
        dialogueDisplay = dialogueUI.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        objectiveSpawner = GameObject.FindWithTag("ObjectiveSpawner").GetComponent<ObjectiveSpawner>();

        objectiveSpawner.OnObjectiveCreated += (text) => CreateDialogue(text);

        dialogueUI.SetActive(false);

        instance = this;
    }

    void Update() {
        if (dialogueUI.activeSelf && Input.GetMouseButton(0)) {
            EndDialogue();
        }
    }

    public void EndDialogue() {
        dialogueDisplay.text = "";
        dialogueUI.SetActive(false);
        onDialogueEnd?.Invoke();
    }
}