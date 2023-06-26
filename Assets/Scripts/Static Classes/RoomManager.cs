using UnityEngine;

public static class RoomManager
{
  public static GameObject SceneTransitionOverlayPrefab;

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void OnAfterSceneLoad()
  {
    SceneTransitionOverlayPrefab = Resources.Load<GameObject>("Prefabs/scene-transition-overlay");
  }

  public static void LoadCompoundRoomByName(string toSceneName)
  {
    GameObject sceneTransitionOverlay = GameObject.Instantiate(SceneTransitionOverlayPrefab, Vector3.zero, Quaternion.identity);
    sceneTransitionOverlay.GetComponent<SceneTransitionOverlay>().LoadWithTransition(toSceneName);
  }
}