using UnityEngine;

public class MushroomPuzzleSetup : MonoBehaviour
{
    public MushroomWithTextUI[] ciuperciImportante;
    public GameObject ciobDeActivat; // 🆕

    void Start()
    {
        MushroomWithTextUI.ciuperciImportante = ciuperciImportante;
        MushroomWithTextUI.ciobDeActivatStatic = ciobDeActivat; // 🆕
    }
}

