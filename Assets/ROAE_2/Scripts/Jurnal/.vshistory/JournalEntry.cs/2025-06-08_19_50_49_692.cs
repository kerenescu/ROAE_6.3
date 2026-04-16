using UnityEngine;

public class JournalEntryTrigger : MonoBehaviour
{
    [Header("Setări intrare în jurnal")]
    [Tooltip("Pagina care va fi adăugată în jurnal")]
    public JournalPageData pageToAdd;

    [Tooltip("Flag-ul care activează această pagină")]
    public DialogueFlag triggerFlag;

    [Tooltip("Se execută o singură dată și apoi componenta se distruge")]
    public bool autoDestroyAfterAdd = true;

    private bool pageWasAdded = false;

    public void TryAddPage()
    {
        if (triggerFlag != null && triggerFlag.IsTriggered())
        {
            if (JournalUIFlow.Instance != null && pageToAdd != null)
            {
                JournalUIFlow.Instance.AddPageIfNotPresent(pageToAdd);

                if (autoDestroyAfterAdd)
                    Destroy(this);
            }
        }
    }

}
