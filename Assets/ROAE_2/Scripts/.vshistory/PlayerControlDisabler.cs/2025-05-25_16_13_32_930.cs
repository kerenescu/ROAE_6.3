using UnityEngine;
using AC;

public class PlayerControlDisabler : MonoBehaviour
{
    private GameObject playerObject;

    private void Awake()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogWarning("❌ Nu s-a găsit obiectul Player!");
        }
    }

    public void DisablePlayer()
    {
        if (playerObject != null)
        {
            foreach (MonoBehaviour comp in playerObject.GetComponents<MonoBehaviour>())
            {
                comp.enabled = false;
            }

            playerObject.GetComponent<Rigidbody2D>()?.simulated.Equals(false);
            playerObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // optional: fade out vizual
        }

        Debug.Log("🔒 Player dezactivat complet");
    }

    public void EnablePlayer()
    {
        if (playerObject != null)
        {
            foreach (MonoBehaviour comp in playerObject.GetComponents<MonoBehaviour>())
            {
                comp.enabled = true;
            }

            playerObject.GetComponent<Rigidbody2D>()?.simulated.Equals(true);
            playerObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        Debug.Log("🔓 Player reactivat complet");
    }
}
