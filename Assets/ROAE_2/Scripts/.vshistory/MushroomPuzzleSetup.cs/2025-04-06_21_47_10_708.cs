using UnityEngine;

public class MushroomPuzzleSetup : MonoBehaviour
{
    public MushroomWithTextUI[] ciuperciImportante;

    void Start()
    {
        MushroomWithTextUI.ciuperciImportante = ciuperciImportante;
    }
}
