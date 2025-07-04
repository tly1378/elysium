using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro support
using System.Collections;


public class PostAnimationDialogueManager : MonoBehaviour
{
   public TextMeshProUGUI npcDialogueText; // Text box for NPC
   public TextMeshProUGUI playerDialogueText; // Text box for Player
   public GameObject npcDialogueBox; // Dialogue box for NPC
   public GameObject playerDialogueBox; // Dialogue box for Player
   public float typingSpeed = 0.05f; // Speed of typing effect
   public AudioSource npcDialogueAudioSource; // Assign in the Inspector
   public AudioClip npcDialogueSoundEffect; // Assign NPC's voice effect in the Inspector


  




   private string[] dialogues = {
       "You are finally awake.",
       "You've been sleeping for a few hours here.",
       "Go home, we are closed for the day.",
       "Where...where am I...?",
       "I gotta go home...god damn headache..."
   };
   private int dialogueIndex = 0; // To track the current dialogue


   private bool isPlayerTurn = false; // Switch between NPC and Player dialogues


   void Start()
   {
       // Ensure the dialogue boxes are hidden at the start
       npcDialogueBox.SetActive(false);
       playerDialogueBox.SetActive(false);
   }


   public void StartPostAnimationDialogue()
   {
       // Start the dialogue sequence after the animation ends
       StartCoroutine(DisplayDialogue());
   }
   


   private IEnumerator DisplayDialogue()
{
    while (dialogueIndex < dialogues.Length)
    {
        // Specific dialogue assignment based on dialogue index
        if (dialogueIndex < 3) // First three lines are NPC
        {
            npcDialogueBox.SetActive(true);
            playerDialogueBox.SetActive(false);
            yield return StartCoroutine(TypeText(npcDialogueText, dialogues[dialogueIndex]));
        }
        else // Last two lines are Player
        {
            playerDialogueBox.SetActive(true);
            npcDialogueBox.SetActive(false);
            yield return StartCoroutine(TypeText(playerDialogueText, dialogues[dialogueIndex]));
        }

        dialogueIndex++; // Move to the next dialogue

        // Wait for player input before continuing
        yield return new WaitUntil(() => Input.anyKeyDown);
    }

    // Hide dialogue boxes after the sequence ends
    npcDialogueBox.SetActive(false);
    playerDialogueBox.SetActive(false);
}


   private IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
{
    textComponent.text = ""; // Clear the text

    if (npcDialogueAudioSource != null && npcDialogueSoundEffect != null)
    {
        npcDialogueAudioSource.clip = npcDialogueSoundEffect;
        npcDialogueAudioSource.loop = true; // Loop sound while typing
        npcDialogueAudioSource.Play();
    }

    foreach (char letter in text.ToCharArray())
    {
        textComponent.text += letter;
        yield return new WaitForSeconds(typingSpeed);
    }

    if (npcDialogueAudioSource != null)
    {
        npcDialogueAudioSource.Stop(); // Stop the sound when typing ends
    }
}


}


