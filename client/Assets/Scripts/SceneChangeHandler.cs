using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChangeHandler : MonoBehaviour
{
    public float delay = 2f; // Delay after fade-to-black
    public string sceneToLoad; // Name of the next scene to load
    private bool isFadeComplete = false;

    public AudioSource backgroundMusic; // Add reference to background music
    public float musicFadeDuration = 2f; // Duration to fade out the music

    private void Update()
    {
        // Check if fade-to-black has completed, then start the scene transition
        if (isFadeComplete)
        {
            StartCoroutine(FadeOutMusicAndLoadScene());
            isFadeComplete = false; // Prevent multiple invokes
        }
    }

    // This method should be called when the screen has fully faded to black
    public void OnFadeComplete()
    {
        Debug.Log("Fade-to-black complete. Scene transition will start in " + delay + " seconds.");
        isFadeComplete = true;
    }

    private IEnumerator FadeOutMusicAndLoadScene()
    {
        float timer = 0f;
        float startVolume = backgroundMusic != null ? backgroundMusic.volume : 0f;

        while (timer < musicFadeDuration)
        {
            if (backgroundMusic != null)
            {
                backgroundMusic.volume = Mathf.Lerp(startVolume, 0, timer / musicFadeDuration);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure the volume is set to 0 before transitioning
        if (backgroundMusic != null) backgroundMusic.volume = 0f;

        yield return new WaitForSeconds(delay); // Wait before changing scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
