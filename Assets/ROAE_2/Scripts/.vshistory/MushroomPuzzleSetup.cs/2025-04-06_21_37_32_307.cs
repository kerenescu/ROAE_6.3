using UnityEngine;

public class PuzzleSetup : MonoBehaviour
{
    public MushroomWithTextUI[] ciupercileImportante;

    void Start()
    {
        MushroomWithTextUI.ciuperciImportante = ciupercileImportante;
    }
}
