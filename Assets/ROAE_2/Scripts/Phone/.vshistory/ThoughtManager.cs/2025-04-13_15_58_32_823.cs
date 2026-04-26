using UnityEngine;

public class ThoughtManager : MonoBehaviour
{
    public GameObject thoughtPrefab;
    public Transform spawnLocation;

    private GameObject currentBubble; // ✅ ținem evidența gândului activ

    public void ShowThought(string text)
    {
        // dacă există deja un gând, îl distrugem
        if (currentBubble != null)
            Destroy(currentBubble);

        // creăm unul nou
        currentBubble = Instantiate(thoughtPrefab, spawnLocation);
        currentBubble.GetComponent<ThoughtBubble>().SetText(text);
    }
}
