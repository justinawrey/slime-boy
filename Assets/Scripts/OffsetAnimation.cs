using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OffsetAnimation : MonoBehaviour
{
  private Animator animator;

  private void Awake()
  {
    animator = GetComponent<Animator>();
  }

  private void Start()
  {
    float randomStart = Random.Range(0, animator.GetCurrentAnimatorStateInfo(0).length);
    animator.Play("Idle", 0, randomStart);
  }
}
