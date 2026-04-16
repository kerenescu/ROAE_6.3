using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 50, 0); // Schimb? valorile dac? vrei alt? direc?ie

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
