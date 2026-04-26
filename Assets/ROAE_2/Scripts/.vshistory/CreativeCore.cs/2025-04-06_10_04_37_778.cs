using UnityEngine;

public class CreativeCore : MonoBehaviour
{
    public static CreativeCore Instance;

    [Header("Valori principale")]
    [Range(0, 100)]
    public int creativity = 50;

    [Range(-5, 5)]
    public int empathy = 0;

    [Range(0, 5)]
    public int plantCorruption = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // rămâne între scene
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
        plantCorruption = Mathf.Clamp(plantCorruption + amount, 0, 5);
    }

    // Opțional: Debug rapid
    public void PrintStats()
    {
        Debug.Log($"🎨 Creativitate: {creativity} | 💗 Empatie: {empathy} | 🪴 Corupție: {plantCorruption}");
    }
}
