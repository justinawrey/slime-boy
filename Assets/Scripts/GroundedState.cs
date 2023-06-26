using UnityEngine;

public class GroundedState : BasePlayerState
{
  public GroundedState(PlayerController ctx) : base(ctx) { }

  public static string RunningSubState = "Grounded.Running";
  public static string IdleSubState = "Grounded.Idle";
  public static string LandingSubState = "Grounded.Landing";

  // Allow some buffer time when player changes direction quickly so we dont go running -> idle -> running
  private Leeway turnAroundBuffer = new Leeway(0.3f);

  public override void Enter()
  {
    PlayerController.HasCloudStep = false;
    PlayerController.NumJumps = 0;
    ctx.SetSubstate(LandingSubState);
  }

  public override void Update()
  {
    if (!ctx.CheckGround())
    {
      ctx.SetState(PlayerController.airbornState);
      return;
    }

    if (ctx.jumpBuffer.Valid())
    {
      // Then jump and switch to airborn state
      ctx.ApplyJumpForce(ctx.jumpForce);
      ctx.SetState(PlayerController.airbornState);
      return;
    }

    ctx.needsToApplyJumpCut = false;

    // Landing substate cannot be interrupted until callback from the animator controller is called.
    bool landing = PlayerController.playerStates.currentSubstate == LandingSubState;
    if (!landing)
    {
      ResetSubstate();
    }
  }

  public override void ResetSubstate()
  {
    // Otherwise, it takes 0.5 seconds in the "idle zone" for idle state to come into effect
    // The "idle zone" is approximately when x velocity is between -0.5 and 0.5.
    bool isIdle = Mathf.Abs(ctx.rb.velocity.x) <= ctx.idleMovemovementThreshold;
    if (isIdle)
    {
      turnAroundBuffer.Tick(Time.deltaTime);
    }
    else
    {
      turnAroundBuffer.Reset();
    }

    if (turnAroundBuffer.Valid())
    {
      ctx.SetSubstate(RunningSubState);
    }
    else
    {
      ctx.SetSubstate(IdleSubState);
    }
  }

  public override void FixedUpdate()
  {
    float targetSpeed = ctx.horizontalAxis * ctx.maxVelocity;
    float accelRate = accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? ctx.runAcceleration : ctx.runDeceleration;
    float speedDiff = targetSpeed - ctx.rb.velocity.x;
    float movement = speedDiff * accelRate;

    ctx.rb.AddForce(movement * Vector2.right);
    if (ctx.horizontalAxis == 0)
    {
      ApplyStoppingFriction();
    }
  }

  private void ApplyStoppingFriction()
  {
    float amount = Mathf.Min(Mathf.Abs(ctx.rb.velocity.x), ctx.friction);
    amount *= Mathf.Sign(ctx.rb.velocity.x);
    ctx.rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
  }
}