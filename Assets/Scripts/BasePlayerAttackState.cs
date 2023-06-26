using UnityEngine;

public class BasePlayerAttackState
{
  public PlayerAttackController ctx;
  public BasePlayerAttackState(PlayerAttackController ctx)
  {
    this.ctx = ctx;
  }

  public virtual void Update() { }
  public virtual void Enter() { }
  public virtual void FixedUpdate() { }

  public virtual void ResetAttackState()
  {
    ctx.SetAttackState(PlayerAttackController.noAttackState);
  }
}