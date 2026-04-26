using UnityEngine;

public class TarotCard : MonoBehaviour
{
    [SerializeField] private GameObject front;
    [SerializeField] private GameObject back;

    private bool isRevealed = false;

    public void OnClick()
    {
        if (isRevealed) return;
        isRevealed = true;

        front.SetActive(true);
        back.SetActive(false);
        TarotReadingManager.Instance.NotifyCardRevealed();
    }
}
