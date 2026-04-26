using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private GameObject dialogueUIRoot;


    [Header("Dialogue System")]
    [SerializeField] private string dialogueLine;
    [SerializeField] private List<DialogueChoice> choices;


    [Header("Portrait System")]
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private VisualNovelPortraits portraitManager;

    


    private DialogueData _currentDialogue;
    private int currentLineIndex = 0;
    private IReadOnlyList<DialogueLine> currentLines;

    public void StartDialogue(DialogueData startingDialogue)
    {
        if (startingDialogue == null)
        {
            Debug.LogError("Starting dialogue is null!");
            return;
        }

        dialogueUIRoot.SetActive(true);
        Time.timeScale = 0f;

        _currentDialogue = startingDialogue;

        portraitManager.ShowCharacters(leftSprite, rightSprite);
        portraitManager.HighlightSpeaker(false); // false = Barista

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_currentDialogue == null) return;

        dialogueText.text = _currentDialogue.DialogueLine;
        ClearChoices();

        foreach (var choice in _currentDialogue.Choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            DialogueChoice capturedChoice = choice; // evită closure bugs
            button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
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
            _currentDialogue = selectedChoice.NextDialogue;
            UpdateUI();
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

        dialogueUIRoot.SetActive(false);
        portraitManager.HideAll();

        Time.timeScale = 1f;
        Debug.Log("Dialogue ended.");
    }
}
