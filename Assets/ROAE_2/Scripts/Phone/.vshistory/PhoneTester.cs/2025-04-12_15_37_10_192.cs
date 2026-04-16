using UnityEngine;

public class PhoneTester : MonoBehaviour
{
    void Start()
    {
        phoneUI.SetActive(false);

        // Test: Mesaje la pornire
        var msg1 = new PhoneMessage("Alex", "Hei! Bine ai venit în joc!", true);
        var msg2 = new PhoneMessage("Unknown", "Ne vom revedea curând...");

        ReceiveMessage(msg1);
        ReceiveMessage(msg2);
    }

}
