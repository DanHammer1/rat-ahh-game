using UnityEngine;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
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

        objectiveSpawner.OnObjectiveCreated += (objective) => CreateDialogue(objective.GetDialogueText());

        dialogueUI.SetActive(false);

        onDialogueActivate += () => {
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
        };
        onDialogueEnd += () => {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        };

        instance = this;
    }

    public void EndDialogue() {
        dialogueDisplay.text = "";
        dialogueUI.SetActive(false);
        onDialogueEnd?.Invoke();
    }
}