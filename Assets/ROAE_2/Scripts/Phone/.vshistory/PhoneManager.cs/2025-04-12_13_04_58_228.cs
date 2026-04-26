
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;
    public Transform messageParent;
    public GameObject messageBubblePrefab;

    public void TogglePhone()
    {
        phoneUI.SetActive(!phoneUI.activeSelf);
    }

    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<Text>().text = messageText;
    }
}
