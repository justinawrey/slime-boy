using UnityEngine;

public class SceneTransitionTrigger : MonoBehaviour
{
  private BoxCollider2D boxCollider;
  public string ToSceneName;
  public Dir ExitDir;

  private void Awake()
  {
    boxCollider = GetComponent<BoxCollider2D>();
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    RoomManager.LoadCompoundRoomByName(ToSceneName);
  }
}
