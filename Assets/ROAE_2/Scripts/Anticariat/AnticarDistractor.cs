using UnityEngine;

public class AnticarDistractor : MonoBehaviour
{
    public GameObject anticar;
    public Transform startPosition;
    public Transform distractionPoint;
    public DialogueFlag distractionFlag;
    public float moveSpeed = 2f;
    public float distractionDuration = 5f;

    private bool isDistracted = false;
    private float distractionTimer = 0f;
    private Vector3 targetPosition;

    void Start()
    {
        if (anticar != null && startPosition != null)
            anticar.transform.position = startPosition.position;

        targetPosition = startPosition.position;
    }

    void Update()
    {
        // Verificăm dacă trebuie pornită distragerea
        if (!isDistracted && distractionFlag != null && distractionFlag.IsTriggered())
        {
            StartDistraction();
        }

        // Mișcare smooth spre target
        if (anticar != null && targetPosition != null)
        {
            anticar.transform.position = Vector3.MoveTowards(
                anticar.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }

        // Cronometru de distragere
        if (isDistracted)
        {
            distractionTimer -= Time.deltaTime;
            if (distractionTimer <= 0f)
            {
                ReturnToStart();
            }
        }
    }

    void StartDistraction()
    {
        targetPosition = distractionPoint.position;
        isDistracted = true;
        distractionTimer = distractionDuration;
        Debug.Log("📚 Anticarul merge să investigheze zgomotul...");
    }

    void ReturnToStart()
    {
        targetPosition = startPosition.position;
        isDistracted = false;
        Debug.Log("🔙 Anticarul revine la locul lui.");
    }
}
