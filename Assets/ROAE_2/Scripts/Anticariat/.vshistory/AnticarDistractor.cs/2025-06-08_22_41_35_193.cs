using UnityEngine;

public class AnticarDistractor : MonoBehaviour
{
    public GameObject anticar;
    public Transform locatieDistragere;
    public float timpDistragere = 8f;
    public DialogueFlag distractionFlag;

    private bool isDistracted = false;
    private float timer = 0f;

    void Update()
    {
        if (!isDistracted && distractionFlag != null && distractionFlag.IsTriggered())
        {
            DistrageAnticarul();
        }

        if (isDistracted)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                anticar.transform.position = transform.position; // revine la poziția originală
                isDistracted = false;
                Debug.Log("📚 Anticarul s-a întors la locul său.");
            }
        }
    }

    void DistrageAnticarul()
    {
        if (anticar != null && locatieDistragere != null)
        {
            anticar.transform.position = locatieDistragere.position;
            timer = timpDistragere;
            isDistracted = true;
            Debug.Log("🪄 Anticarul este distras.");
        }
    }
}
