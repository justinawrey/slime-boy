using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PulsingLight : MonoBehaviour
{
  private Light2D light2d;

  // Distance covered per second along X axis of Perlin plane.
  [SerializeField] private float pulseSpeed = 1.0f;
  [SerializeField] private float pulseRange = 3.0f;
  private float originalOuterRadius;
  private float offset;

  private void Awake()
  {
    light2d = GetComponent<Light2D>();
    originalOuterRadius = light2d.pointLightOuterRadius;
    offset = Random.Range(0, 100);
  }

  private void Update()
  {
    float clamped01 = Mathf.Clamp01(Mathf.PerlinNoise(offset + Time.time * pulseSpeed, offset + Time.time * pulseSpeed));
    float normal = Mathf.InverseLerp(0, 1, clamped01);
    float valueInPulseRange = Mathf.Lerp(-pulseRange, pulseRange, normal);
    light2d.pointLightOuterRadius = originalOuterRadius + valueInPulseRange;
  }
}
