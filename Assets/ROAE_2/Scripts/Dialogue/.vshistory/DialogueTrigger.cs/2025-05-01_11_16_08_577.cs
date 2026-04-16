public void TriggerDialogue()
{
    Debug.Log("🎯 TriggerDialogue() a fost apelată!");

    if (dialogueManager && dialogue)
    {
        Debug.Log("✅ Pornim dialogul pentru: " + gameObject.name);
        dialogueManager.StartDialogue(dialogue);
    }
    else
    {
        Debug.LogWarning("❌ DialogueManager sau DialogueData lipsesc la: " + gameObject.name);
    }
}
