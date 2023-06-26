using UnityEngine;

public class PlayDialogueSequence : MonoBehaviour
{
  private void OnTriggerEnter2D(Collider2D other)
  {
    StartCoroutine(DialogueManager.StartDialogueSequence("TestDialogueSequence"));
  }
}
