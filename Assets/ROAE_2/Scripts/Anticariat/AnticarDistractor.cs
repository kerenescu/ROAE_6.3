using AC;
using UnityEngine;

public class AnticarDistractor : MonoBehaviour
{
    public GameObject anticar;
    public Transform startPosition;
    public Transform distractionPoint;
    public DialogueFlag distractionFlag;
    public float moveSpeed = 2f;
    public float distractionDuration = 5f;

    [SerializeField] private string acVariableName = "AnticarulPrezent";
    [SerializeField] private bool debugLogs = true;

    private const float ArrivalSqrDistance = 0.04f;

    private bool isEscaping;
    private bool escapeApplied;

    private void Start()
    {
        if (anticar == null)
            anticar = gameObject;

        if (distractionFlag != null && distractionFlag.IsTriggered())
        {
            ApplyEscapeState();
            return;
        }

        if (anticar != null && startPosition != null)
            anticar.transform.position = startPosition.position;

        SetAnticarPresence(true);
    }

    private void Update()
    {
        if (escapeApplied || anticar == null)
            return;

        if (!isEscaping)
        {
            if (distractionFlag != null && distractionFlag.IsTriggered())
                StartEscape();

            return;
        }

        if (distractionPoint == null)
        {
            ApplyEscapeState();
            return;
        }

        anticar.transform.position = Vector3.MoveTowards(
            anticar.transform.position,
            distractionPoint.position,
            moveSpeed * Time.deltaTime);

        if ((anticar.transform.position - distractionPoint.position).sqrMagnitude <= ArrivalSqrDistance)
            ApplyEscapeState();
    }

    private void StartEscape()
    {
        if (escapeApplied)
            return;

        isEscaping = true;
        SetAnticarPresence(false);
        Log("Anticar is running after Gilbert.");
    }

    private void ApplyEscapeState()
    {
        if (escapeApplied)
            return;

        escapeApplied = true;
        isEscaping = false;

        SetAnticarPresence(false);

        if (anticar != null)
            anticar.SetActive(false);

        Log("Anticar has left the shop.");
    }

    private void SetAnticarPresence(bool isPresent)
    {
        if (string.IsNullOrWhiteSpace(acVariableName))
            return;

        GVar acVar = GlobalVariables.GetVariable(acVariableName);
        if (acVar == null || acVar.BooleanValue == isPresent)
            return;

        acVar.BooleanValue = isPresent;
        acVar.Upload();
    }

    private void Log(string message)
    {
        if (!debugLogs)
            return;

        Debug.Log("[ROAE][AnticarDistractor] " + message);
    }
}
