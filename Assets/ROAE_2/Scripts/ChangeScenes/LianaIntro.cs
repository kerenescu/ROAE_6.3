using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LianaFallWorld : MonoBehaviour
{
    public float targetY = 0.25f;
    public float speed = 5f;
    public string nextScene = "FlowerField"; // schimbă cu ce vrei tu

    private bool hasLanded = false;

    void Update()
    {
        if (hasLanded) return;

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
        transform.position = pos;

        Debug.Log($"Liana Y: {pos.y}, Target: {targetY}");

        if (Mathf.Abs(pos.y - targetY) < 0.01f)
        {
            hasLanded = true;
            Debug.Log("🎯 Liana a ajuns!");
            StartCoroutine(LoadNextScene());
        }
    }


    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(nextScene);
    }
}
