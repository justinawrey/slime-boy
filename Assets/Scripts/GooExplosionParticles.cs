using UnityEngine;
using System.Collections.Generic;

public class GooExplosionParticles : MonoBehaviour
{
  [SerializeReference] private GameObject gooSplotchPrefab;

  private ParticleSystem particles;
  private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

  private void Awake()
  {
    particles = GetComponent<ParticleSystem>();
  }

  private void OnParticleCollision(GameObject other)
  {
    particles.GetCollisionEvents(other, collisionEvents);

    foreach (ParticleCollisionEvent collisionEvent in collisionEvents)
    {
      var splotch = Instantiate(gooSplotchPrefab, collisionEvent.intersection, Quaternion.identity);

      // Then this splotch is facing up, do no rotation
      if (collisionEvent.normal.y == 1)
      {
        continue;
      }

      // Hit the ceiling
      if (collisionEvent.normal.y == -1)
      {
        splotch.transform.Rotate(new Vector3(0, 0, 1), 180);
        continue;
      }

      // Hit a leftwards wall
      if (collisionEvent.normal.x == -1)
      {
        splotch.transform.Rotate(new Vector3(0, 0, 1), 90);
        continue;
      }

      // Hit a rightwards wall
      if (collisionEvent.normal.x == 1)
      {
        splotch.transform.Rotate(new Vector3(0, 0, 1), -90);
        continue;
      }
    }
  }
}