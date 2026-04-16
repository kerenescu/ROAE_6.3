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

    [Header("Hint System")]
    [SerializeField] private GameObject spaceHintText;
    [SerializeField] private float hintDelaySeconds = 2f;


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
    private float timeSinceLineShown = 0f; /* pentru hint */

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
        currentLines = _currentDialogue.DialogueLines;
        currentLineIndex = 0;

        portraitManager.ShowCharacters(leftSprite, rightSprite);
        ShowNextLine();
    }

    public void ShowNextLine()
    {
        ClearChoices();

        if (currentLineIndex >= currentLines.Count)
        {
            ShowChoices();
            return;
        }

        DialogueLine line = currentLines[currentLineIndex];
        dialogueText.text = line.Text;

        // Highlight speaker
        if (line.Speaker.ToLower().Contains("rina"))
            portraitManager.HighlightSpeaker(true); // Rina e pe dreapta
        else
            portraitManager.HighlightSpeaker(false); // Barista sau altul pe stânga

        currentLineIndex++;

        spaceHintText.SetActive(false);
        timeSinceLineShown = 0f;

    }

    private void ShowChoices()
    {
        foreach (var choice in _currentDialogue.Choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            DialogueChoice capturedChoice = choice;
            button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }

    private void UpdateUI()
    {
        if (_currentDialogue == null) return;

        dialogueText.text = _currentDialogue.DialogueLines[0].Text;

        ClearChoices();

        foreach (var choice in _currentDialogue.Choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            DialogueChoice capturedChoice = choice; // evită closure bugs
            button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }

        if (_currentDialogue == null || Time.timeScale == 1f) return;

        // detectăm apăsare SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowNextLine();
            spaceHintText.SetActive(false);
        }
        else
        {
            timeSinceLineShown += Time.unscaledDeltaTime;

            if (timeSinceLineShown >= hintDelaySeconds && !spaceHintText.activeSelf)
            {
                spaceHintText.SetActive(true);
            }
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
