using UnityEngine;

public class PhoneBootstrapper : MonoBehaviour
{
    public GameObject phoneSystemPrefab;

    void Awake()
    {
        if (FindObjectOfType<PhoneManager>() == null)
        {
            Instantiate(phoneSystemPrefab);
        }
    }
}
