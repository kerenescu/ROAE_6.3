using UnityEngine;

public class UsaTrigger : MonoBehaviour
{
    public GameObject usaDeschisa;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 clickPos = new Vector2(worldPos.x, worldPos.y);

            Collider2D hit = Physics2D.OverlapPoint(clickPos);

            if (hit != null && hit.gameObject == this.gameObject)
            {
                DeschideUsa();
            }
        }
    }

    void DeschideUsa()
    {
        if (usaDeschisa != null)
        {
            usaDeschisa.SetActive(true);
            gameObject.SetActive(false); // ascunde triggerul, opțional
        }
    }
}
