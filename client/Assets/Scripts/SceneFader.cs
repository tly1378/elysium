using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class SceneFader : MonoBehaviour
{
    public Image fadeScreen; // Reference to the UI Image used for fading
    public float fadeDuration = 2f; // Duration of the fade

    private void Start()
    {
        if (fadeScreen != null)
        {
            // Ensure the fade screen starts fully transparent
            fadeScreen.color = new Color(0, 0, 0, 0);
            fadeScreen.gameObject.SetActive(false);
        }
    }

    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    private IEnumerator FadeOut(string sceneName)
    {
        fadeScreen.gameObject.SetActive(true);
        Color fadeColor = fadeScreen.color;
        float elapsedTime = 0f;

        // Gradually increase the alpha value
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeScreen.color = fadeColor;
            yield return null;
        }

        // Load the new scene
        SceneManager.LoadScene(sceneName);
    }
}
