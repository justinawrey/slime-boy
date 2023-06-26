using UnityEngine;

public class StateObserver : MonoBehaviour
{
  public virtual void Awake()
  {
    PlayerController.SubstateChanged += OnSubstateChanged;
    PlayerAttackController.AttackStateChanged += OnAttackStateChanged;
  }

  public virtual void OnDestroy()
  {
    PlayerController.SubstateChanged -= OnSubstateChanged;
    PlayerAttackController.AttackStateChanged -= OnAttackStateChanged;
  }

  public virtual void OnAttackStateChanged(PlayerAttackStates playerAttackStates)
  {
    // Deconstruct
    BasePlayerAttackState currentState = playerAttackStates.currentState;
    BasePlayerAttackState prevState = playerAttackStates.prevState;

    if (currentState.GetType() == typeof(BashState))
    {
      OnBash();
    }
  }

  public virtual void OnSubstateChanged(PlayerStates playerStates)
  {
    // Deconstruct
    BasePlayerState currentState = playerStates.currentState;
    BasePlayerState prevState = playerStates.prevState;
    string currentSubstate = playerStates.currentSubstate;
    string prevSubstate = playerStates.prevSubstate;

    // Landed
    if (currentSubstate == GroundedState.LandingSubState)
    {
      OnLanded();
    }

    // Started running
    if (currentSubstate == GroundedState.RunningSubState)
    {
      OnRunning();
    }

    // Stopped running
    if (currentSubstate == GroundedState.IdleSubState)
    {
      OnIdle();
    }

    // Falling
    if (currentSubstate == AirbornState.FallingSubState)
    {
      OnFalling();
    }

    // Take off
    if (currentSubstate == AirbornState.UpwardsSubState)
    {
      OnTakeOff();
    }

    if (currentSubstate == AirbornState.UpwardsSecondJumpSubState)
    {
      OnSecondJump();
    }

    if (currentSubstate == AirbornState.HoveringSubstate)
    {
      OnHovering();
    }

    if (currentSubstate != AirbornState.HoveringSubstate)
    {
      OnNotHovering();
    }
  }

  public virtual void OnTakeOff() { }
  public virtual void OnFalling() { }
  public virtual void OnLanded() { }
  public virtual void OnRunning() { }
  public virtual void OnIdle() { }
  public virtual void OnBash() { }
  public virtual void OnSecondJump() { }
  public virtual void OnHovering() { }
  public virtual void OnNotHovering() { }
}