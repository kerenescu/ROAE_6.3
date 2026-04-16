using UnityEngine;
using TMPro;


public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI; // Obiectul PhoneUI (întregul widget)
    public Transform messageParent; // Content din Scroll View
    public GameObject messageBubblePrefab; // Prefab-ul cu bulă + text

    public void TogglePhone()
    {
        Debug.Log("Apasat butonul de telefon");

        bool newState = !phoneUI.activeSelf;
        phoneUI.SetActive(newState);

        Debug.Log("Telefonul este acum " + (newState ? "deschis" : "inchis"));
    }


    public void ReceiveMessage(string messageText)
    {
        GameObject bubble = Instantiate(messageBubblePrefab, messageParent);
        bubble.GetComponentInChildren<TextMeshProUGUI>().text = messageText;

    }
}
