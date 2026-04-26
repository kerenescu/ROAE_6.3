using System.Collections.Generic;
using UnityEngine;

public class ResponseMessageTest : MonoBehaviour
{
    private bool triggered = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !triggered)
        {
            triggered = true;

            List<string> raspunsuri = new List<string>
            {
                "Da, am ajuns!",
                "Nu încă.",
                "Sunt pierdută."
            };

            PhoneMessage mesajInteractiv = new PhoneMessage("Elina", "Ai ajuns acolo în siguranță?", false);
            mesajInteractiv.hasChoices = true;
            mesajInteractiv.responseOptions = raspunsuri;


            MessageManager mm = FindObjectOfType<MessageManager>();
            if (mm != null)
            {
                mm.ReceiveMessage(mesajInteractiv);
            }
            else
            {
                Debug.LogWarning("MessageManager nu a fost găsit în scenă!");
            }
        }
    }
}
