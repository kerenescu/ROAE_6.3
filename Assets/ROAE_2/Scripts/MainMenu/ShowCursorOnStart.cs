using UnityEngine;

public class ShowCursorOnStart : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // Debug.Log("🖱️ Cursorul a fost făcut vizibil.");
    }
}
