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

            // 🔥 Definim opțiunile de răspuns cu efecte asupra stats
            List<DecisionChoice> choices = new List<DecisionChoice>
            {
                new DecisionChoice("Da, am ajuns!", new StatsEffect { creativity = +5 }),
                new DecisionChoice("Nu încă.", new StatsEffect { empathy = -1 }),
                new DecisionChoice("Sunt pierdută.", new StatsEffect { anxiety = +2 }) // dacă adaugi anxiety în StatsEffect
            };

            // 🔥 Mesaj cu decizie
            PhoneMessage mesajInteractiv = new PhoneMessage("Elina", "Ai ajuns acolo în siguranță?", choices);

            // 🔥 Trimitem mesajul în MessageManager
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
