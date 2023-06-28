using UnityEngine;
using Cinemachine;
using System.Collections;

public class VirtualCameraEffects : MonoBehaviour
{
  private CinemachineVirtualCamera vCam;
  private float maxNoise = 1f;
  private float minNoise = 0f;
  private float initialXDamping;
  private float initialYDamping;
  private float initialZDamping;
  public float noiseLerpDuration = 2f;

  // Start lerping to max camera noise only after idleBuffer amount of seconds
  public float idleBuffer = 1f;
  private float idleBufferCounter = 0;

  private CinemachineFramingTransposer transposer;
  private CinemachineBasicMultiChannelPerlin noise;
  // private float initialYDamping;
  // public float fallDamping = 0f;
  // public float fallDampingLerpDuration = 0.5f;

  [Header("Screen shake options")]
  public float shakeNoiseFrequency = 5f;
  public float shakeNoiseIntensity = 5f;
  public float shakeDuration = 0.5f;
  private float initialNoiseFrequency;

  private void Start()
  {
    vCam = GetComponent<CinemachineVirtualCamera>();
    transposer = vCam.GetCinemachineComponent<CinemachineFramingTransposer>();
    // noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    initialXDamping = transposer.m_XDamping;
    initialYDamping = transposer.m_YDamping;
    initialZDamping = transposer.m_ZDamping;
    // initialNoiseFrequency = noise.m_FrequencyGain;
  }

  private void Update()
  {
    // ApplyIdleNoise();
    // ApplyFallDampingMultiplier();
  }

  private void ApplyIdleNoise()
  {
    // if there is no noise, add some noise
    if (noise.m_AmplitudeGain == minNoise)
    {
      if (IsIdle())
      {
        idleBufferCounter += Time.deltaTime;
      }
      else
      {
        idleBufferCounter = 0f;
      }

      if (idleBufferCounter > idleBuffer)
      {
        StartCoroutine(LerpNoiseRoutine(maxNoise, noiseLerpDuration));
        idleBufferCounter = 0;
      }
    }
    // If there is noise (we are fully idle), then lerp down when starting to move 
    else if (noise.m_AmplitudeGain == maxNoise)
    {
      if (!IsIdle())
      {
        StartCoroutine(LerpNoiseRoutine(minNoise, noiseLerpDuration / 2));
      }
    }
  }

  // private void ApplyFallDampingMultiplier()
  // {
  //   if (transposer.m_YDamping == initialYDamping)
  //   {
  //     if (IsFalling())
  //     {
  //       StartCoroutine(LerpYDampingRoutine(fallDamping, fallDampingLerpDuration));
  //     }
  //   }
  //   else if (transposer.m_YDamping == fallDamping)
  //   {
  //     if (!IsFalling())
  //     {
  //       StartCoroutine(LerpYDampingRoutine(initialYDamping, fallDampingLerpDuration));
  //     }
  //   }
  // }

  private bool IsIdle()
  {
    return PlayerController.playerStates.currentState.GetType() == typeof(GroundedState) && PlayerController.playerStates.currentSubstate == GroundedState.IdleSubState;
  }

  // private bool IsFalling()
  // {
  //   return PlayerController.currentState.GetType() == typeof(AirbornState) && PlayerController.subState == AirbornState.FallingSubState;
  // }

  private IEnumerator LerpNoiseRoutine(float endValue, float duration)
  {
    float time = 0;
    float startValue = noise.m_AmplitudeGain;
    while (time < duration)
    {
      noise.m_AmplitudeGain = Mathf.Lerp(startValue, endValue, time / duration);
      time += Time.deltaTime;
      yield return null;
    }
    noise.m_AmplitudeGain = endValue;
  }

  // private IEnumerator LerpYDampingRoutine(float endValue, float duration)
  // {
  //   float time = 0;
  //   float startValue = transposer.m_YDamping;
  //   while (time < duration)
  //   {
  //     transposer.m_YDamping = Mathf.Lerp(startValue, endValue, time / duration);
  //     time += Time.deltaTime;
  //     yield return null;
  //   }
  //   transposer.m_YDamping = endValue;
  // }

  public IEnumerator ShakeRoutine()
  {
    noise.m_FrequencyGain = shakeNoiseFrequency;
    float originalIntensity = noise.m_AmplitudeGain;
    yield return StartCoroutine(LerpNoiseRoutine(shakeNoiseIntensity, shakeDuration / 2));
    yield return StartCoroutine(LerpNoiseRoutine(originalIntensity, shakeDuration / 2));
    noise.m_FrequencyGain = initialNoiseFrequency;
  }

  public void DisableDamping()
  {
    print("HI");
    transposer.m_XDamping = 0;
    transposer.m_YDamping = 0;
    transposer.m_ZDamping = 0;
  }

  public void EnableDamping()
  {
    print("BYE");
    transposer.m_XDamping = initialXDamping;
    transposer.m_YDamping = initialYDamping;
    transposer.m_ZDamping = initialZDamping;
  }

  public void ForcePosition(Vector2 pos)
  {
    // Retain the z position and tracked object offset when forcing to an x,y coord
    vCam.ForceCameraPosition(new Vector3(pos.x, pos.y, transform.position.z) + transposer.m_TrackedObjectOffset, Quaternion.identity);
  }
}