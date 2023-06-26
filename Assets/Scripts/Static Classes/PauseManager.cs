using UnityEngine;

public static class PauseManager
{
  public static GameObject PauseMenuPrefab;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void OnAfterSceneLoad()
  {
    PauseMenuPrefab = Resources.Load<GameObject>("Prefabs/pause-menu");
  }

  public static void Pause()
  {
    Time.timeScale = 0;
    ActionMapManager.PushMap(ActionMapManager.PausedActionMapName);
    GameObject.Instantiate(PauseMenuPrefab, Vector3.zero, Quaternion.identity);
  }

  public static void Unpause()
  {
    Time.timeScale = 1;
    ActionMapManager.PopMap();

    GameObject pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu");
    if (pauseMenu != null)
    {
      GameObject.Destroy(pauseMenu);
    }
  }
}