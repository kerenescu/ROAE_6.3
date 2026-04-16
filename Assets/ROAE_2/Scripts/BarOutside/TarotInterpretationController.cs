using UnityEngine;
using TMPro;

public class TarotInterpretationPanelController : MonoBehaviour
{
    public static TarotInterpretationPanelController Instance;

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI interpretationText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        panelRoot.SetActive(false);
    }

    public void ShowInterpretation(string text)
    {
        interpretationText.text = text;
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }
}
