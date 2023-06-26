public class BasePlayerState
{
  public PlayerController ctx;
  public BasePlayerState(PlayerController ctx)
  {
    this.ctx = ctx;
  }

  public virtual void Update() { }
  public virtual void Enter() { }
  public virtual void FixedUpdate() { }
  public virtual void ResetSubstate() { }
}