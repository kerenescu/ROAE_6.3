using UnityEngine;

public class TarotCard : MonoBehaviour
{
    [SerializeField] private GameObject backImage;
    [SerializeField] private GameObject frontImage;
    private bool isRevealed = false;

    private void OnMouseDown()
    {
        if (isRevealed) return;

        isRevealed = true;
        backImage.SetActive(false);
        frontImage.SetActive(true);

        TarotReadingManager.Instance.NotifyCardRevealed();
    }
}
