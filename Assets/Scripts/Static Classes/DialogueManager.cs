using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public static class DialogueManager
{
  public static Dictionary<char, GameObject> CharPrefabLookup;
  public static DialogueBox DialogueBoxPrefab;
  public static Vector3 DialogBoxPosition = new Vector3(0f, -5.5f, 0f);

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void OnAfterSceneLoad()
  {
    CharPrefabLookup = Resources.Load<CharPrefabDictSO>("Scriptable Objects/CharPrefabLookupSO").CharPrefabDict;
    DialogueBoxPrefab = Resources.Load<DialogueBox>("Prefabs/dialogue-box");
  }

  public static DialogueSequenceSO GetDialogueSequenceByName(string name)
  {
    return Resources.Load<DialogueSequenceSO>($"Scriptable Objects/{name}");
  }

  public static IEnumerator StartDialogueSequence(string name)
  {
    Debug.Log($"dialogue manager: starting dialog sequence {name}");

    DialogueSequenceSO dialogueSequence = GetDialogueSequenceByName(name);

    ActionMapManager.PushMap(ActionMapManager.DialogueActionMapName);
    InputAction nextLineAction = ActionMapManager.MainInputActionAsset.FindActionMap(ActionMapManager.DialogueActionMapName).FindAction("Next");

    // Do something!
    DialogueBox box = GameObject.Instantiate<DialogueBox>(DialogueBoxPrefab, DialogBoxPosition, Quaternion.identity);
    foreach (DialogueLine dialogueLine in dialogueSequence.DialogueLines)
    {
      box.Clear();
      box.StartTypeWriting(dialogueLine.Content);

      // Wait until the action unpresses first, so we don't run through all of the text in a single press. 
      yield return new WaitUntil(() => !nextLineAction.IsPressed());
      yield return new WaitUntil(() => nextLineAction.WasPressedThisFrame());

      if (box.IsTypeWriting())
      {
        box.FinishTypeWriting();
        yield return new WaitUntil(() => !nextLineAction.IsPressed() && !box.IsTypeWriting());
        yield return new WaitUntil(() => nextLineAction.WasPressedThisFrame());
      }
    }

    GameObject.Destroy(box.gameObject);
    ActionMapManager.PopMap();
  }
}