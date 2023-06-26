using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public static class ActionMapManager
{
  public static Stack<InputActionMap> ActionMapStack = new Stack<InputActionMap>();
  public static InputActionAsset MainInputActionAsset;
  public static string PlayerActionMapName = "Player";
  public static string DialogueActionMapName = "Dialogue";
  public static string PausedActionMapName = "Paused";
  public static string UIActionMapName = "UI";

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void OnAfterSceneLoad()
  {
    MainInputActionAsset = Resources.Load<InputActionAsset>("Input Action Assets/MainInputActionAsset");

    // For now, the player action map is the default one.
    InputActionMap playerActionMap = MainInputActionAsset.FindActionMap(PlayerActionMapName);
    playerActionMap.Enable();
    ActionMapStack.Push(playerActionMap);
  }

  // Enables given action map, disables the previous
  public static void PushMap(string name)
  {
    ActionMapStack.Peek().Disable();
    InputActionMap newMap = MainInputActionAsset.FindActionMap(name);
    ActionMapStack.Push(newMap);
    newMap.Enable();
    Debug.Log($"action map manager: action map switched to {name}");
  }

  // Disable the enabled map, go back to the previous
  public static void PopMap()
  {
    InputActionMap previous = ActionMapStack.Pop();
    previous.Disable();
    InputActionMap _new = ActionMapStack.Peek();
    _new.Enable();
    Debug.Log($"action map manager: action map switched to {_new.name}");
  }

  // Find an action from a given action map
  public static InputAction FindActionFromMap(string actionMap, string action)
  {
    return MainInputActionAsset.FindActionMap(actionMap).FindAction(action);
  }
}