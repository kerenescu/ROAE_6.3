using UnityEngine;

public class PhoneTester : MonoBehaviour
{
    void Start()
    {
        FindObjectOfType<PhoneManager>().ReceiveMessage("✨ Ai primit un mesaj nou!");
    }
}
