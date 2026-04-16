using UnityEngine;
using System.Collections.Generic; // 🔧 avem nevoie de List<string>

public class PhoneTester : MonoBehaviour
{
    public PhoneManager phoneManager;

    void Start()
    {
        // ✅ Mesaj normal, conversație nouă, fără opțiuni
        PhoneMessage msg1 = new PhoneMessage("???:", "Salut, Rina!");

        // ✅ Mesaj cu multiple choice
        List<string> raspunsuri = new List<string> { "Cine ești?", "Ce vrei?", "Ok..." };
        PhoneMessage msg2 = new PhoneMessage("???:", "Ai primit un mesaj misterios.", false, true, raspunsuri);

        // ✅ Mesaj vechi (conversație din trecut)
        PhoneMessage msg3 = new PhoneMessage("Prietenul tău", "Ți-am zis eu că o să reușești!", true);

        // Trimite-le în telefon
        phoneManager.ReceiveMessage(msg1);
        phoneManager.ReceiveMessage(msg2);
        phoneManager.ReceiveMessage(msg3);
    }
}
