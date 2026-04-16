using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LianaFallWorld : MonoBehaviour
{
    public float targetY = 0f;
    public float speed = 5f;
    public string nextScene = "FlowerField"; // schimbă cu ce vrei tu

    private bool hasLanded = false;

    void Update()
    {
        if (hasLanded) return;

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
        transform.position = pos;

        if (Mathf.Approximately(pos.y, targetY))
        {
            hasLanded = true;
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadScene(nextScene);
    }
}
