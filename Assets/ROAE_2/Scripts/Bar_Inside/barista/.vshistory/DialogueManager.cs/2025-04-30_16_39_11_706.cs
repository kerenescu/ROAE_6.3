using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private Button choiceButtonPrefab;
    [SerializeField] private GameObject dialogueUIRoot;

    [Header("Portrait System")]
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private VisualNovelPortraits portraitManager;

    private DialogueData _currentDialogue;

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
            Button button
