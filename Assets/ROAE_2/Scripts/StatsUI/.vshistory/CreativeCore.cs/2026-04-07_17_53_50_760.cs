using UnityEngine;

public class CreativeCore : MonoBehaviour
{
    public static CreativeCore Instance;

    [Header("Valori principale")]
    [Range(0, 100)]
    public int creativity = 50;

    [Range(-5, 5)]
    public int empathy = 0;

    [Range(0, 10)]
    public int plantCorruption = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[STATS][CreativeCore.Awake] instance ready");
        }
        else
        {
            Debug.Log("[STATS][CreativeCore.Awake] duplicate instance destroyed");
            Destroy(gameObject);
        }
    }

    public void AdjustCreativity(int amount)
    {
        int before = creativity;
        creativity = Mathf.Clamp(creativity + amount, 0, 100);
        Debug.Log("[STATS][AdjustCreativity] before=" + before + " delta=" + amount + " after=" + creativity);
    }

    public void AdjustEmpathy(int amount)
    {
        int before = empathy;
        empathy = Mathf.Clamp(empathy + amount, -5, 5);
        Debug.Log("[STATS][AdjustEmpathy] before=" + before + " delta=" + amount + " after=" + empathy);
    }

    public void AdjustCorruption(int amount)
    {
        int before = plantCorruption;
        plantCorruption = Mathf.Clamp(plantCorruption + amount, 0, 10);
        Debug.Log("[STATS][AdjustCorruption] before=" + before + " delta=" + amount + " after=" + plantCorruption);
    }

    public void PrintStats()
    {
        Debug.Log("[STATS][PrintStats] creativity=" + creativity + " empathy=" + empathy + " plantCorruption=" + plantCorruption);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[STATS][Hotkey] key=T action=Creativity+10");
            AdjustCreativity(10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("[STATS][Hotkey] key=Y action=Empathy+1");
            AdjustEmpathy(1);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("[STATS][Hotkey] key=U action=Corruption+1");
            AdjustCorruption(1);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[STATS][Hotkey] key=G action=Creativity-10");
            AdjustCreativity(-10);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("[STATS][Hotkey] key=H action=Empathy-1");
            AdjustEmpathy(-1);
            PrintStats();
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("[STATS][Hotkey] key=J action=Corruption-1");
            AdjustCorruption(-1);
            PrintStats();
        }
    }
}
