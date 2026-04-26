using UnityEngine;

public class CreativeCore : MonoBehaviour
{
    public static CreativeCore Instance;

    [Header("Main values")]
    [Range(0, 100)] public int creativity = 50;
    [Range(-5, 5)] public int empathy = 0;
    [Range(0, 10)] public int plantCorruption = 0;

    [Header("Debug")]
    [SerializeField] private bool enableDebugHotkeys = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdjustCreativity(int amount)
    {
        creativity = Mathf.Clamp(creativity + amount, 0, 100);
    }

    public void AdjustEmpathy(int amount)
    {
        empathy = Mathf.Clamp(empathy + amount, -5, 5);
    }

    public void AdjustCorruption(int amount)
    {
        plantCorruption = Mathf.Clamp(plantCorruption + amount, 0, 10);
    }

    public void PrintStats()
    {
        Debug.Log("Creativity: " + creativity + " | Empathy: " + empathy + " | Corruption: " + plantCorruption);
    }

    void Update()
    {
        if (!enableDebugHotkeys)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            AdjustCreativity(10);
            Debug.Log("Creativity +10");
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            AdjustEmpathy(1);
            Debug.Log("Empathy +1");
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            AdjustCorruption(1);
            Debug.Log("Corruption +1");
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            AdjustCreativity(-10);
            Debug.Log("Creativity -10");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            AdjustEmpathy(-1);
            Debug.Log("Empathy -1");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            AdjustCorruption(-1);
            Debug.Log("Corruption -1");
        }
    }
}
