using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TarotCard : MonoBehaviour
{
    [SerializeField] private GameObject back;
    [SerializeField] private GameObject front;
    [TextArea][SerializeField] private string interpretation;

    private bool isRevealed = false;
    private bool awaitingSpace = false;

    private void Start()
    {
        front.SetActive(false);
        back.GetComponent<Button>().onClick.AddListener(OnBackClick);
    }

    private void Update()
    {
        if (awaitingSpace && Input.GetKeyDown(KeyCode.Space))
        {
            TarotReadingManager.Instance.SetMadameText("");
            awaitingSpace = false;
            TarotReadingManager.Instance.NotifyCardRevealed();
        }
    }

    private void OnClick()
    {
        if (isRevealed || awaitingSpace)
            return;

        StartCoroutine(RevealCard());
    }

    private IEnumerator RevealCard()
    {
        back.SetActive(false);
        front.SetActive(true);
        isRevealed = true;

        TarotReadingManager.Instance.SetMadameText(interpretation);

        yield return new WaitForSeconds(1.5f); // timp blocat

        awaitingSpace = true;
    }
}
