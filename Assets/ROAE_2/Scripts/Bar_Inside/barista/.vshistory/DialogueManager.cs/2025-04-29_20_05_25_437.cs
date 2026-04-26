using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public Text dialogueText;
    public GameObject choicesContainer;
    public Button choiceButtonPrefab;

    [System.Serializable]
    public class DialogueChoice
    {
        public string text;
        public UnityEngine.Events.UnityEvent onChoiceSelected;
    }

    [System.Serializable]
    public class DialogueData
    {
        public string dialogueLine;
        public List<DialogueChoice> choices;
    }

    public DialogueData currentDialogue;

    private void Start()
    {
        ShowDialogue(currentDialogue);
    }

    public void ShowDialogue(DialogueData dialogue)
    {
        dialogueText.text = dialogue.dialogueLine;
        ClearChoices();

        foreach (var choice in dialogue.choices)
        {
            Button newButton = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            newButton.GetComponentInChildren<Text>().text = choice.text;
            newButton.onClick.AddListener(() => { choice.onChoiceSelected.Invoke(); ClearChoices(); });
        }
    }

    private void ClearChoices()
    {
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
