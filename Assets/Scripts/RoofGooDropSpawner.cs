using UnityEngine;
using System.Collections;

public class RoofGooDropSpawner : MonoBehaviour
{
  [SerializeField] private ParticleSystem particles;
  [SerializeField] private float minSpawnInterval = 2f;
  [SerializeField] private float maxSpawnInterval = 8f;

  private Animator animator;
  private Coroutine spawnRoutine;

  private void Awake()
  {
    animator = GetComponent<Animator>();
  }

  private void Start()
  {
    spawnRoutine = StartCoroutine(SpawnDrops());
  }

  // Called by animation event
  public void SpawnParticle()
  {
    particles.Emit(1);
    animator.Play("Idle");
  }

  private IEnumerator SpawnDrops()
  {
    while (true)
    {
      float interval = Random.Range(minSpawnInterval, maxSpawnInterval);
      yield return new WaitForSeconds(interval);
      animator.Play("Drop");
    }
  }

  private void OnDisable()
  {
    StopCoroutine(spawnRoutine);
  }
}