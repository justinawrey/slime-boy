using System.Collections;
using UnityEngine;

public class LetterEffects : MonoBehaviour
{
  private SpriteRenderer spriteRenderer;
  private float onePx = 1f / 16f;
  [SerializeField] private Material waveMaterial;
  [SerializeField] private Material fadeOutMaterial;
  [SerializeField] private float shakeInterval;
  private Coroutine shakeRoutine;
  private Coroutine fadeOutRoutine;

  private void Awake()
  {
    spriteRenderer = GetComponent<SpriteRenderer>();
  }

  public void AddEffect(LetterEffect effect)
  {
    switch (effect)
    {
      case LetterEffect.Shake:
        shakeRoutine = StartCoroutine(ShakeRoutine());
        break;

      case LetterEffect.FadeOut:
        spriteRenderer.material = fadeOutMaterial;
        fadeOutMaterial.SetFloat("_StartTime", Time.time);
        StartCoroutine(DestroyRoutine());
        break;

      case LetterEffect.Wave:
        spriteRenderer.material = waveMaterial;
        break;

      case LetterEffect.None:
      default:
        break;
    }
  }

  private IEnumerator DestroyRoutine()
  {
    yield return new WaitForSeconds(1f / fadeOutMaterial.GetFloat("_SpeedMultiplier"));
    Destroy(gameObject);
  }

  private IEnumerator ShakeRoutine()
  {
    Vector2[] possibleOffsets = new Vector2[] {
        new Vector2(-onePx, onePx),
        new Vector2(0, onePx),
        new Vector2(-onePx, 0),
        new Vector2(0, 0),
    };
    int totalPositions = possibleOffsets.Length;
    Vector3 originalPosition = transform.position;

    while (true)
    {
      Vector2 randomOffset = possibleOffsets[Random.Range(0, totalPositions)];
      transform.position = originalPosition + new Vector3(randomOffset.x, randomOffset.y, 0);
      yield return new WaitForSeconds(shakeInterval);
    }
  }

  private void OnDisable()
  {
    if (shakeRoutine != null)
    {
      StopCoroutine(shakeRoutine);
    }
  }
}
