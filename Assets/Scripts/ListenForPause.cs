using UnityEngine;
using UnityEngine.InputSystem;

public class ListenForPause : MonoBehaviour
{
  [SerializeField] string mapName;
  private InputAction pauseAction;

  private void Start()
  {
    pauseAction = ActionMapManager.FindActionFromMap(mapName, "Pause");
  }

  private void Update()
  {
    if (pauseAction.WasPerformedThisFrame())
    {
      PauseManager.Pause();
    }
  }
}
