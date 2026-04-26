using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;

    private DialogueData currentDialogue;

    private void Start()
    {
        // Aici poți seta manual primul DialogueData sau îl pornești din alt script.
    }

    public void StartDialogue(DialogueData startingDialogue)
    {
        currentDialogue = startingDialogue;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentDialogue == null)
        {
            Debug.LogWarning("DialogueManager: No dialogue to display.");
            return;
        }

        dialogueText.text = currentDialogue.DialogueLine;
        ClearChoices();

        foreach (var choice in currentDialogue.Choices)
        {
            Button newButton = Instantiate(choiceButtonPrefab, choicesContainer);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;
            DialogueChoice capturedChoice = choice; // evită closure issues
            newButton.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void OnChoiceSelected(DialogueChoice selectedChoice)
    {
        if (selectedChoice.NextDialogue != null)
        {
            StartDialogue(selectedChoice.NextDialogue);
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        dialogueText.text = "";
        ClearChoices();
        Debug.Log("Dialogue ended.");
    }
}
