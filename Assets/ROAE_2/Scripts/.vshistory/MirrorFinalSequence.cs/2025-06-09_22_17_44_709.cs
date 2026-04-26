using UnityEngine;

public class MirrorFinalSequence : MonoBehaviour
{
    [Header("Obiecte necesare")]
    public GameObject jesterflor;
    public GameObject oglindaGlowEffect;

    [Header("Nume iteme din inventar")]
    public string ciob1Name = "ciob1";
    public string ciob2Name = "ciob2";
    public string ciob3Name = "ciob3";

    [Header("Activare o singură dată")]
    public bool triggerOnce = true;
    private bool hasTriggered = false;

    void Start()
    {
        CheckInventoryAndTrigger();
    }

    public void CheckInventoryAndTrigger()
    {
        if (hasTriggered && triggerOnce) return;

        if (AC.KickStarter.runtimeInventory.PlayerInvCollection.Contains(ciob1Name) &&
            AC.KickStarter.runtimeInventory.PlayerInvCollection.Contains(ciob2Name) &&
            AC.KickStarter.runtimeInventory.PlayerInvCollection.Contains(ciob3Name))
        {
            // Activează efect glow pe oglindă
            if (oglindaGlowEffect != null)
                oglindaGlowEffect.SetActive(true);

            // Activează Jesterflor
            if (jesterflor != null)
                jesterflor.SetActive(true);

            Debug.Log("🌟 Toate cioburile sunt în inventar. Oglinda a fost activată, iar Jesterflor a apărut.");

            hasTriggered = true;
        }
        else
        {
            Debug.Log("🔍 Nu ai toate cele 3 cioburi încă.");
        }
    }
}
