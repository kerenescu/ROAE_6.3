using UnityEngine;

public class ThoughtManager : MonoBehaviour
{
    public GameObject thoughtPrefab;
    public Transform spawnLocation;

    public void ShowThought(string text)
    {
        GameObject bubble = Instantiate(thoughtPrefab, spawnLocation);
        bubble.GetComponent<ThoughtBubble>().SetText(text);
    }
}
