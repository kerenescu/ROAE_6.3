using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LianaIntroAnimation : MonoBehaviour
{
    public RectTransform lianaTransform;
    public float targetY = -100f;
    public float speed = 300f;
    public string nextSceneName = "FlowerField"; // sau cum se numește prima scenă

    private bool hasReached = false;

    void Start()
    {
        if (lianaTransform == null)
            lianaTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (hasReached) return;

        Vector2 pos = lianaTransform.anchoredPosition;
        pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
        lianaTransform.anchoredPosition = pos;

        if (Mathf.Approximately(pos.y, targetY))
        {
            hasReached = true;
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(nextSceneName);
    }
}
