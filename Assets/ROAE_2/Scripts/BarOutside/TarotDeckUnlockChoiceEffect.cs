using UnityEngine;

[CreateAssetMenu(fileName = "TarotDeckUnlockChoiceEffect", menuName = "ROAE/Bar Exterior/Tarot Deck Unlock Choice Effect")]
public class TarotDeckUnlockChoiceEffect : DialogueChoiceEffect
{
    [SerializeField] private DialogueFlag tarotUnlockFlag;
    [SerializeField] private bool debugLogs = true;

    public override void Apply()
    {
        if (tarotUnlockFlag != null)
            tarotUnlockFlag.MarkAsTriggered();

        TarotDeck deck = FindTarotDeckInScene();
        if (deck == null)
        {
            Debug.LogWarning("[ROAE][TarotDeckUnlockChoiceEffect] TarotDeck not found in loaded scene.");
            return;
        }

        deck.UnlockFromDialogue();

        if (debugLogs)
        {
            Debug.Log(
                "[ROAE][TarotDeckUnlockChoiceEffect] Tarot deck unlocked" +
                " | flag=" + (tarotUnlockFlag != null ? tarotUnlockFlag.name : "NULL") +
                " | deck=" + deck.name);
        }
    }

    private static TarotDeck FindTarotDeckInScene()
    {
        TarotDeck[] decks = Resources.FindObjectsOfTypeAll<TarotDeck>();
        foreach (TarotDeck deck in decks)
        {
            if (deck == null)
                continue;

            if (!deck.gameObject.scene.IsValid() || !deck.gameObject.scene.isLoaded)
                continue;

            return deck;
        }

        return null;
    }
}
