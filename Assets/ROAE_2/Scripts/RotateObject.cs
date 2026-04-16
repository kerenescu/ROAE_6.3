using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    void Start()
    {
        // Asigură-te că scriptul e activ la pornire
        this.enabled = true;
    }

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
