using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator : MonoBehaviour
{
    public Sprite image1;  // First image (must be public or serialized)
    public Sprite image2;  // Second image

    private Image imageComponent;

    void Start()
    {
        imageComponent = GetComponent<Image>(); 
        StartCoroutine(AnimateImages());
    }

    IEnumerator AnimateImages()
    {
        while (true)
        {
            imageComponent.sprite = image1; // Set first image
            yield return new WaitForSeconds(0.5f); // Wait 0.5 seconds
            
            imageComponent.sprite = image2; // Set second image
            yield return new WaitForSeconds(0.5f); // Wait 0.5 seconds
        }
    }
}
