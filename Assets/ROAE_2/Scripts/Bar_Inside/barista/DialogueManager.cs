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

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool skipPortraitCalls = false;

    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private IReadOnlyList<DialogueLine> currentLines;
    private float timeSinceLineShown = 0f;

    private void Awake()
    {
        Log(
            "Awake on object=" + gameObject.name +
            " | dialogueText=" + ObjName(dialogueText) +
            " | choicesContainer=" + ObjName(choicesContainer) +
            " | choiceButtonPrefab=" + ObjName(choiceButtonPrefab) +
            " | dialogueUIRoot=" + ObjName(dialogueUIRoot) +
            " | spaceHintText=" + ObjName(spaceHintText) +
            " | portraitManager=" + ObjName(portraitManager)
        );
    }

    private void Update()
    {
        if (currentDialogue == null)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Log("Space pressed -> ShowNextLine");
            ShowNextLine();

            if (spaceHintText != null)
                spaceHintText.SetActive(false);
        }
        else
        {
            timeSinceLineShown += Time.unscaledDeltaTime;

            if (spaceHintText != null &&
                timeSinceLineShown >= hintDelaySeconds &&
                !spaceHintText.activeSelf &&
                !AreChoicesVisible())
            {
                spaceHintText.SetActive(true);
            }
        }
    }

    public void StartDialogue(DialogueData startingDialogue)
    {
        try
        {
            Log("StartDialogue called on object=" + gameObject.name);

            if (startingDialogue == null)
            {
                Debug.LogError("[ROAE][DialogueManager] startingDialogue is NULL");
                return;
            }

            Log("startingDialogue=" + startingDialogue.name);

            if (startingDialogue.DialogueLines == null)
            {
                Debug.LogError("[ROAE][DialogueManager] startingDialogue.DialogueLines is NULL");
                return;
            }

            Log("lineCount=" + startingDialogue.DialogueLines.Count);

            if (startingDialogue.DialogueLines.Count == 0)
            {
                Debug.LogWarning("[ROAE][DialogueManager] DialogueData has no lines: " + startingDialogue.name);
                return;
            }

            if (dialogueUIRoot == null)
            {
                Debug.LogError("[ROAE][DialogueManager] dialogueUIRoot is NULL");
                return;
            }

            if (dialogueText == null)
            {
                Debug.LogError("[ROAE][DialogueManager] dialogueText is NULL");
                return;
            }

            LogRootState("Before UI enable");

            dialogueUIRoot.SetActive(true);

            LogRootState("After UI enable");

            if (!dialogueUIRoot.activeInHierarchy)
            {
                Debug.LogWarning(
                    "[ROAE][DialogueManager] dialogueUIRoot activeSelf=true but activeInHierarchy=false. Disabled parent chain=" +
                    GetParentChain(dialogueUIRoot.transform)
                );
            }

            Time.timeScale = 0f;

            currentDialogue = startingDialogue;
            currentLines = currentDialogue.DialogueLines;
            currentLineIndex = 0;
            timeSinceLineShown = 0f;

            if (portraitManager != null)
            {
                Log("portraitManager found: " + portraitManager.name);

                if (!skipPortraitCalls)
                {
                    Log("Calling ShowCharacters");
                    portraitManager.ShowCharacters(leftSprite, rightSprite);
                    Log("ShowCharacters finished");
                }
                else
                {
                    Log("skipPortraitCalls=true -> ShowCharacters skipped");
                }
            }
            else
            {
                Debug.LogWarning("[ROAE][DialogueManager] portraitManager is NULL");
            }

            Log("Calling ShowNextLine");
            ShowNextLine();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ROAE][DialogueManager] EXCEPTION in StartDialogue: " + ex);
        }
    }

    public void ShowNextLine()
    {
        try
        {
            Log("ShowNextLine called | index=" + currentLineIndex);

            ClearChoices();

            if (spaceHintText != null)
                spaceHintText.SetActive(false);

            timeSinceLineShown = 0f;

            if (currentLines == null)
            {
                Debug.LogError("[ROAE][DialogueManager] currentLines is NULL");
                return;
            }

            if (currentLineIndex >= currentLines.Count)
            {
                Log("No more lines -> ShowChoices");
                ShowChoices();
                return;
            }

            DialogueLine line = currentLines[currentLineIndex];
            if (line == null)
            {
                Debug.LogWarning("[ROAE][DialogueManager] Current line is NULL at index=" + currentLineIndex);
                currentLineIndex++;
                ShowNextLine();
                return;
            }

            string speaker = line.Speaker != null ? line.Speaker : "";
            string text = line.Text != null ? line.Text : "";

            Log("Line data | speaker=" + speaker + " | text=" + text);

            dialogueText.text = text;
            Log("dialogueText updated");

            bool isRina = speaker.ToLower().Contains("rina");
            Log("isRina=" + isRina);

            if (portraitManager != null && !skipPortraitCalls)
            {
                Log("Calling HighlightSpeaker");
                portraitManager.HighlightSpeaker(!isRina);
                Log("HighlightSpeaker finished");
            }

            currentLineIndex++;

            bool isLastLine = currentLineIndex >= currentLines.Count;
            bool hasChoices = currentDialogue != null &&
                              currentDialogue.Choices != null &&
                              currentDialogue.Choices.Count > 0;

            Log("After line | currentLineIndex=" + currentLineIndex + " | isLastLine=" + isLastLine + " | hasChoices=" + hasChoices);

            if (isLastLine && isRina && hasChoices)
            {
                Log("Last line belongs to Rina and choices exist -> ShowChoices");
                ShowChoices();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ROAE][DialogueManager] EXCEPTION in ShowNextLine: " + ex);
        }
    }

    private void ShowChoices()
    {
        try
        {
            Log("ShowChoices called");

            if (spaceHintText != null)
                spaceHintText.SetActive(false);

            if (currentDialogue == null || currentDialogue.Choices == null || currentDialogue.Choices.Count == 0)
            {
                Log("No choices -> EndDialogue");
                EndDialogue();
                return;
            }

            if (choicesContainer == null)
            {
                Debug.LogError("[ROAE][DialogueManager] choicesContainer is NULL");
                EndDialogue();
                return;
            }

            if (choiceButtonPrefab == null)
            {
                Debug.LogError("[ROAE][DialogueManager] choiceButtonPrefab is NULL");
                EndDialogue();
                return;
            }

            dialogueText.text = "";

            if (portraitManager != null && !skipPortraitCalls)
                portraitManager.HighlightSpeaker(false);

            foreach (DialogueChoice choice in currentDialogue.Choices)
            {
                if (choice == null)
                    continue;

                Button button = Instantiate(choiceButtonPrefab, choicesContainer);
                TextMeshProUGUI tmp = button.GetComponentInChildren<TextMeshProUGUI>();

                if (tmp != null)
                    tmp.text = choice.ChoiceText;
                else
                    Debug.LogWarning("[ROAE][DialogueManager] Choice button has no TMP text child");

                DialogueChoice capturedChoice = choice;
                button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
            }

            Log("Choices spawned=" + currentDialogue.Choices.Count);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ROAE][DialogueManager] EXCEPTION in ShowChoices: " + ex);
        }
    }

    private void OnChoiceSelected(DialogueChoice selectedChoice)
    {
        try
        {
            Log("Choice selected");

            if (selectedChoice == null)
            {
                Debug.LogWarning("[ROAE][DialogueManager] Selected choice is NULL");
                EndDialogue();
                return;
            }

            if (selectedChoice.StatEffect != null)
                selectedChoice.StatEffect.Apply();

            if (selectedChoice.RelationshipEffect != null)
                selectedChoice.RelationshipEffect.Apply();

            if (selectedChoice.ExtraEffects != null)
            {
                foreach (DialogueChoiceEffect effect in selectedChoice.ExtraEffects)
                {
                    if (effect != null)
                        effect.Apply();
                }
            }

            if (selectedChoice.NextDialogue != null)
                StartDialogue(selectedChoice.NextDialogue);
            else
                EndDialogue();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ROAE][DialogueManager] EXCEPTION in OnChoiceSelected: " + ex);
        }
    }

    private void EndDialogue()
    {
        try
        {
            Log("EndDialogue");

            if (dialogueText != null)
                dialogueText.text = "";

            ClearChoices();

            if (spaceHintText != null)
                spaceHintText.SetActive(false);

            if (dialogueUIRoot != null)
                dialogueUIRoot.SetActive(false);

            if (portraitManager != null && !skipPortraitCalls)
                portraitManager.HideAll();

            Time.timeScale = 1f;

            currentDialogue = null;
            currentLines = null;
            currentLineIndex = 0;
            timeSinceLineShown = 0f;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ROAE][DialogueManager] EXCEPTION in EndDialogue: " + ex);
        }
    }

    private void ClearChoices()
    {
        if (choicesContainer == null)
            return;

        foreach (Transform child in choicesContainer)
            Destroy(child.gameObject);
    }

    private bool AreChoicesVisible()
    {
        return choicesContainer != null && choicesContainer.childCount > 0;
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[ROAE][DialogueManager] " + message);
    }

    private void LogRootState(string prefix)
    {
        if (dialogueUIRoot == null)
        {
            Log(prefix + " | dialogueUIRoot=NULL");
            return;
        }

        Log(
            prefix +
            " | root=" + dialogueUIRoot.name +
            " | activeSelf=" + dialogueUIRoot.activeSelf +
            " | activeInHierarchy=" + dialogueUIRoot.activeInHierarchy +
            " | parentChain=" + GetParentChain(dialogueUIRoot.transform)
        );
    }

    private static string GetParentChain(Transform target)
    {
        if (target == null)
            return "NULL";

        string chain = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            chain = current.name + " -> " + chain;
            current = current.parent;
        }

        return chain;
    }

    private static string ObjName(Object obj)
    {
        return obj != null ? obj.name : "NULL";
    }
}
