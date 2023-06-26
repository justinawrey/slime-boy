public class NoAttackState : BasePlayerAttackState
{
  public NoAttackState(PlayerAttackController ctx) : base(ctx) { }

  public override void Update()
  {
    if (ctx.bashActionDown)
    {
      ctx.SetAttackState(PlayerAttackController.bashState);
    }
  }
}