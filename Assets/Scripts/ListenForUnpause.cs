using UnityEngine;
using UnityEngine.InputSystem;

public class ListenForUnpause : MonoBehaviour
{
  private InputAction unpauseAction;

  private void Start()
  {
    unpauseAction = ActionMapManager.FindActionFromMap(ActionMapManager.PausedActionMapName, "Unpause");
  }

  private void Update()
  {
    if (unpauseAction.WasPerformedThisFrame())
    {
      PauseManager.Unpause();
    }
  }
}