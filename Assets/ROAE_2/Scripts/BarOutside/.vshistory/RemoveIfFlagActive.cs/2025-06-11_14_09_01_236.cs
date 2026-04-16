using UnityEngine;

public class RemoveIfFlagActive : MonoBehaviour
{
    [Tooltip("Numele flagului care trebuie sa fie activ pentru a o elimina pe Madame Lichenia")]
    public string flagName;

    [Tooltip("Daca e bifat, obiectul va fi DEZACTIVAT. Daca nu, va fi DISTRUS.")]
    public bool disableInsteadOfDestroy = true;

    void Start()
    {
        // Presupunem ca ai un manager de flaguri persistent in scena
        if (PersistentFlags.IsFlagActive(flagName))
        {
            if (disableInsteadOfDestroy)
                gameObject.SetActive(false);  // Dispare din scena, dar nu se distruge
            else
                Destroy(gameObject);          // Dispare complet din memorie
        }
    }
}
