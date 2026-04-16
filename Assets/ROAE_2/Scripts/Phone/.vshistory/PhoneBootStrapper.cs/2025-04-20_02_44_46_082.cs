using UnityEngine;

public class PhoneBootstrapper : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject phoneSystemPrefab;

    private static bool phoneInstantiated = false;

    void Awake()
    {
        if (!phoneInstantiated)
        {
            if (phoneSystemPrefab == null)
            {
                Debug.LogError("❌ PhoneBootstrapper: Nu ai setat prefab-ul pentru PhoneSystem!");
                return;
            }

            GameObject phone = Instantiate(phoneSystemPrefab);
            DontDestroyOnLoad(phone);
            phone.name = "PhoneSystem"; // ca să fie clar în hierarchy

            phoneInstantiated = true;
            Debug.Log("✅ PhoneSystem a fost instanțiat de bootstrapper.");
        }
        else
        {
            Debug.Log("ℹ️ PhoneSystem deja instanțiat, nu mai facem nimic.");
        }
    }
}
