using UnityEngine;

public class GooSplotchSpawner : StateObserver
{
  [SerializeField] private GameObject gooSplotchPrefab;

  public override void OnLanded()
  {
    SpawnGooSplotch();
  }

  private void SpawnGooSplotch()
  {
    GameObject splotch = Instantiate(gooSplotchPrefab, transform.position, Quaternion.identity);

    // Make sure the splotch is facing the same direction that the player was facing when spawned.
    // There is a one pixel offset to make it pixel perfect.
    if (!PlayerController.FacingRight)
    {
      splotch.GetComponent<SpriteRenderer>().flipX = true;
    }
  }
}