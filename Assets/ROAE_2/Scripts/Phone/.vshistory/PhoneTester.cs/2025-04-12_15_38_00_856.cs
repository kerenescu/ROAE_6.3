using UnityEngine;

public class PhoneTester : MonoBehaviour
{
    public PhoneManager phoneManager;

    void Start()
    {
        // Trimite mesaje de test
        phoneManager.ReceiveMessage("Salut, Rina!");
        phoneManager.ReceiveMessage("Un mesaj nou!");
    }
}
