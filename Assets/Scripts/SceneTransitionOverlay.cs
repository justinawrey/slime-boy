using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

// The scene loading coroutine must be done on this monobehaviour
// because it will persist across scenes.  If we were to start the scene load
// coroutine from the trigger object, the coroutine would die right when the new scene loads
// and the old scene unloads.
public class SceneTransitionOverlay : MonoBehaviour
{
  [SerializeField] private float transitionDuration = 0.5f;
  private Animator animator;

  private void Awake()
  {
    animator = GetComponent<Animator>();
    DontDestroyOnLoad(gameObject);
  }

  public void LoadWithTransition(string toSceneName)
  {
    StartCoroutine(LoadWithTransitionRoutine(toSceneName));
  }

  private IEnumerator LoadWithTransitionRoutine(string toSceneName)
  {
    string fromRoom = SceneManager.GetActiveScene().name;
    animator.SetBool("SceneVisible", false);

    // Start asynchronous load, wait for a buffer period, finish
    var asyncLoading = AsyncLoader.LoadSceneRoutine(toSceneName);
    yield return new WaitForSeconds(transitionDuration);
    yield return asyncLoading;

    // At this point, the new scene has loaded and the old scene has unloaded.
    // So, we can search the new scene for transition triggers to figure out where
    // our player should be placed.
    MovePlayerToSceneEntrance(fromRoom);

    // Let the transition in animation play out before destroying our overlay
    animator.SetBool("SceneVisible", true);
    yield return new WaitForSeconds(transitionDuration);
    Destroy(gameObject);
  }

  private void MovePlayerToSceneEntrance(string fromRoom)
  {
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    GameObject vCam = GameObject.FindGameObjectWithTag("MainVirtualCamera");
    GameObject grid = GameObject.FindGameObjectWithTag("Grid");
    VirtualCameraEffects vCamEffects = vCam.GetComponent<VirtualCameraEffects>();
    CinemachineConfiner2D confiner = vCam.GetComponent<CinemachineConfiner2D>();
    PolygonCollider2D polygonCollider = grid.GetComponent<PolygonCollider2D>();
    GameObject[] sceneTransitionTriggers = GameObject.FindGameObjectsWithTag("SceneTransitionTrigger");

    foreach (GameObject sceneTransitionTrigger in sceneTransitionTriggers)
    {
      SceneTransitionTrigger transitionComponent = sceneTransitionTrigger.GetComponent<SceneTransitionTrigger>();
      if (transitionComponent.ToSceneName == fromRoom)
      {
        Vector2 offset = GetOffsetOppositeToExitDir(transitionComponent.ExitDir);
        player.transform.position = sceneTransitionTrigger.transform.position + (Vector3)offset;
        confiner.m_BoundingShape2D = polygonCollider;
        confiner.InvalidateCache();
        vCamEffects.ForcePosition(new Vector2(player.transform.position.x, player.transform.position.y));
        return;
      }
    }
  }

  // If exiting to the left, enter 2 units to the right
  // If exiting downwards, enter 2 units above... etc
  private Vector2 GetOffsetOppositeToExitDir(Dir dir)
  {
    switch (dir)
    {
      case Dir.UP:
        return new Vector2(0, -2);
      case Dir.RIGHT:
        return new Vector2(-2, 0);
      case Dir.DOWN:
        return new Vector2(0, 2);
      case Dir.LEFT:
        return new Vector2(2, 0);

      // this should never happen.... silly compiler cant figure this out????
      default:
        return new Vector2(2, 0);
    }
  }

  // uncomment when we need virtual camera support
  // private void MovePlayerToSceneEntrance(string fromRoom)
  // {
  //   GameObject player = GameObject.FindGameObjectWithTag("Player");
  //   GameObject vCam = GameObject.FindGameObjectWithTag("MainVirtualCamera");
  //   VirtualCameraEffects vCamEffects = vCam.GetComponent<VirtualCameraEffects>();
  //   GameObject[] sceneTransitions = GameObject.FindGameObjectsWithTag("SceneTransition");

  //   foreach (GameObject sceneTransition in sceneTransitions)
  //   {
  //     TransitionScene transitionComponent = sceneTransition.GetComponent<TransitionScene>();
  //     if (transitionComponent.toSceneName == fromRoom)
  //     {
  //       Vector2 offset = GetOffsetOppositeToExitDir(transitionComponent.exitDir);
  //       player.transform.position = sceneTransition.transform.position + (Vector3)offset;
  //       vCamEffects.ForcePosition(new Vector2(player.transform.position.x, player.transform.position.y));
  //       return;
  //     }
  //   }
  // }
}
