using UnityEngine;

public class UnlockReveal : MonoBehaviour
{
    [Header("Obiecte de activat")]
    public GameObject usaDeschisa;
    public GameObject ciobul;

    public void RevealEverything()
    {
        if (usaDeschisa != null)
        {
            usaDeschisa.SetActive(true);
            Debug.Log("🔓 Usa deschisa a fost activata.");
        }

        if (ciobul != null)
        {
            ciobul.SetActive(true);
            Debug.Log("✨ Ciobul a fost activat.");
        }
    }
}
