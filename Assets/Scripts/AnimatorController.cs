using UnityEngine;

// Right now, bashing trumps all movement animations.
// We should change it so the entire attack controller
// trumps all movement animations (future once there are more attacks).
public class AnimatorController : StateObserver
{
  private Animator animator;
  private bool bashing = false;

  public void ForceMovementAnimationChange()
  {
    OnSubstateChanged(PlayerController.playerStates);
  }

  public override void Awake()
  {
    base.Awake();
    animator = GetComponent<Animator>();
  }

  public override void OnTakeOff()
  {
    Play("Air Up");
  }

  public override void OnSecondJump()
  {
    Play("Air Up 2");
  }

  public override void OnFalling()
  {
    Play("Air Down");
  }

  public override void OnLanded()
  {
    Play("Landing");
  }

  public override void OnRunning()
  {
    Play("Running");
  }

  public override void OnIdle()
  {
    Play("Idle");
  }

  public override void OnHovering()
  {
    Play("Hover Intro");
  }

  public override void OnBash()
  {
    bashing = true;

    switch (PlayerAttackController.CurrAimDir)
    {
      case Dir.LEFT:
      case Dir.RIGHT:
        animator.Play("Bash Sideways");
        return;
      case Dir.UP:
        animator.Play("Bash Up");
        return;
      case Dir.DOWN:
        animator.Play("Bash Down");
        return;
    }
  }

  private void Play(string animation)
  {
    if (!bashing)
    {
      animator.Play(animation);
    }
  }

  public void HoverIntroFinished()
  {
    Play("Hover Active");
  }

  public void ResetMovementSubstate()
  {
    PlayerController.playerStates.currentState.ResetSubstate();
    PlayerController.ForceCallback();
  }

  public void ResetAttackState()
  {
    bashing = false;
    PlayerAttackController.playerAttackStates.currentState.ResetAttackState();
    ResetMovementSubstate();
  }
}