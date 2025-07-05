using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public Image fadeScreen;
    public float fadeDuration = 2f;

    public PostAnimationDialogueManager dialogueManager;
   



    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeScreen.color;
        color.a = 1f; // Start fully black
        fadeScreen.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            fadeScreen.color = color;
            yield return null;
        }

        color.a = 0f; // Fully transparent
        fadeScreen.color = color;
        fadeScreen.gameObject.SetActive(false); // Optional: Disable the image
    }

    
}


