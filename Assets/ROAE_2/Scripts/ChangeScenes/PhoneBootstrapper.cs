// === PhoneBootstrapper.cs ===
using UnityEngine;

public class PhoneBootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject phoneSystemPrefab;
    private static bool isPhoneSystemInstantiated = false;

    void Awake()
    {
        if (!isPhoneSystemInstantiated)
        {
            GameObject phoneInstance = Instantiate(phoneSystemPrefab);
            DontDestroyOnLoad(phoneInstance);
            isPhoneSystemInstantiated = true;
            Debug.Log("✅ PhoneSystem instantiat si marcat ca DontDestroyOnLoad");
        }
        Destroy(this.gameObject);
    }
}