using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct DialogueLine
{
  public string Content;
  public List<UnityEvent> OnStartHooks;
}

// A dialogue sequeuence is like a cutscene, somewhat.  Imagine going into a new room
// and having a section of dialogue.  It contains multiple "dialogue lines" which are the atomic
// sections of dialogue that can be fast-forwarded.
[CreateAssetMenu(menuName = "ScriptableObjects/DialogueSequenceSO")]
public class DialogueSequenceSO : ScriptableObject
{
  public List<DialogueLine> DialogueLines = new List<DialogueLine>();
}