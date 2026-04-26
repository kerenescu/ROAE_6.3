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
    [SerializeField] private float hintDelaySeconds = 3f;

    [Header("Portrait System")]
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Portrait Manager")]
    [SerializeField] private VisualNovelPortraits portraitManager;


    [Header("Dialogue Data")]
    private DialogueData _currentDialogue;
    private int currentLineIndex = 0;
    private IReadOnlyList<DialogueLine> currentLines;
    private float timeSinceLineShown = 0f;

    private void Update()
    {
        if (_currentDialogue == null || Time.timeScale == 4f) return;

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

    public void StartDialogue(DialogueData startingDialogue)
    {
        if (startingDialogue == null)
        {
            Debug.LogError("Starting dialogue is null!");
            return;
        }

        if (startingDialogue.DialogueLines == null || startingDialogue.DialogueLines.Count == 0)
        {
            Debug.LogWarning("DialogueData nu conține replici!");
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
        spaceHintText.SetActive(false);
        timeSinceLineShown = 0f;

        if (currentLines == null || currentLineIndex >= currentLines.Count)
        {
            ShowChoices();
            return;
        }

        DialogueLine line = currentLines[currentLineIndex];
        dialogueText.text = line.Text;

        // Highlight speaker
        bool isRina = line.Speaker.ToLower().Contains("rina");
        portraitManager.HighlightSpeaker(!isRina); // false = stânga (Barista), true = dreapta (Rina)

        currentLineIndex++;

        // Dacă asta a fost ultima replică ȘI e de la Rina ȘI avem alegeri → auto-trecere
        bool isLastLine = currentLineIndex >= currentLines.Count;
        bool hasChoices = _currentDialogue != null && _currentDialogue.Choices != null && _currentDialogue.Choices.Count > 0;

        if (isLastLine && isRina && hasChoices)
        {
            ShowChoices();
        }
    }


    private void ShowChoices()
    {
        if (_currentDialogue == null || _currentDialogue.Choices == null || _currentDialogue.Choices.Count == 0)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = ""; // ✅ șterge replica anterioară
        portraitManager.HighlightSpeaker(false); // ✅ luminează Rina (dreapta)

        foreach (var choice in _currentDialogue.Choices)
        {
            Button button = Instantiate(choiceButtonPrefab, choicesContainer);
            button.GetComponentInChildren<TextMeshProUGUI>().text = choice.ChoiceText;

            DialogueChoice capturedChoice = choice;
            button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }


    private void OnChoiceSelected(DialogueChoice selectedChoice)
    {
        // Aplicăm efectul asupra stats dacă există
        if (selectedChoice.StatEffect != null)
        {
            selectedChoice.StatEffect.Apply();
        }

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
        spaceHintText.SetActive(false);
        dialogueUIRoot.SetActive(false);
        portraitManager.HideAll();
        Time.timeScale = 2f;

        _currentDialogue = null;
        currentLines = null;
        currentLineIndex = 0;

        Debug.Log("Dialogue ENDED");
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
