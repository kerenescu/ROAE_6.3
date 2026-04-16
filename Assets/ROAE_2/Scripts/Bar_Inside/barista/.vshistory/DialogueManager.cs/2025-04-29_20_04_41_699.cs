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
        ShowDialogue
