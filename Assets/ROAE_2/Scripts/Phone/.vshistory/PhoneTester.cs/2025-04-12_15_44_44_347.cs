using UnityEngine;
using System.Collections.Generic;

public class PhoneTester : MonoBehaviour
{
    public PhoneManager phoneManager;

    void Start()
    {
        // 🔹 Trimite un mesaj simplu
        PhoneMessage msg1 = new PhoneMessage("AI", "Salut, Rina! Ai primit un mesaj nou.");
        phoneManager.ReceiveMessage(msg1);

        // 🔹 Trimite un mesaj cu opțiuni de răspuns
        List<string> raspunsuri = new List<string> { "Expune-o!", "Mai am nevoie de timp...", "Anulează participarea." };
        PhoneMessage msg2 = new PhoneMessage("Curatorul", "Ce vrei să facem cu lucrarea ta?", raspunsuri);
        phoneManager.ReceiveMessage(msg2);
    }
}
