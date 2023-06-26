using UnityEngine;
using System.Collections;

public class AirbornState : BasePlayerState
{
  public AirbornState(PlayerController context) : base(context) { }

  public static string FallingSubState = "Airborn.Falling";
  public static string UpwardsSubState = "Airborn.Upwards";
  public static string UpwardsSecondJumpSubState = "Airborn.UpwardsSecondJump";
  public static string HoveringSubstate = "Airborn.HoveringSubstate";

  private float counter = 0f;
  private bool groundCheckEnabled = true;
  private float velocityBuffer = 0.1f; // To account for weirdness when running off ledge, landing, etc.
  private bool hovering = false;

  public override void Enter()
  {
    ResetSubstate();
    ctx.StartCoroutine(DisableGroundCheckRoutine());
  }

  public override void Update()
  {
    hovering = ctx.hoverActionDown && !ctx.hoverActionUp;

    if (groundCheckEnabled && ctx.CheckGround())
    {
      ctx.SetState(PlayerController.groundedState);
      return;
    }

    ResetNeedsToApplyJumpCut();

    // We ran off a ledge but still tried to jump in coyote time
    if (ctx.jumpActionDown)
    {
      if (ctx.coyoteTime.Valid() || PlayerController.HasCloudStep)
      {
        ctx.ApplyJumpForce(ctx.jumpForce);
      }
    }

    ResetSubstate();
    ApplyJumpCut();
    ApplyFallGravity();
  }

  public override void ResetSubstate()
  {
    // If we're hovering, that overrides other airborn states.
    if (hovering)
    {
      ctx.SetSubstate(HoveringSubstate);
      return;
    }

    bool cloudStepping = PlayerController.NumJumps >= 2;
    float yVel = ctx.rb.velocity.y;

    if (yVel < -velocityBuffer)
    {
      if (!cloudStepping)
      {
        ctx.SetSubstate(FallingSubState);
      }
    }
    else if (yVel > velocityBuffer)
    {
      if (cloudStepping)
      {
        ctx.SetSubstate(UpwardsSecondJumpSubState);
      }
      else
      {
        ctx.SetSubstate(UpwardsSubState);
      }
    }
  }

  public override void FixedUpdate()
  {
    float targetSpeed = ctx.horizontalAxis * ctx.maxVelocity;
    float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? ctx.runAcceleration * ctx.airAcceleration : ctx.runDeceleration * ctx.airDeceleration;
    float speedDiff = targetSpeed - ctx.rb.velocity.x;
    float movement = speedDiff * accelRate;
    ctx.rb.AddForce(movement * Vector2.right);
    LimitFallVelocity();
    ApplyHoveringForce();
  }

  private void ApplyHoveringForce()
  {
    if (!hovering) return;
    if (ctx.rb.velocity.y > ctx.maxUpwardsVelocityWhileHovering) return;

    float hoverForce = ctx.hoverForce;
    if (ctx.rb.velocity.y >= 0)
    {
      hoverForce = ctx.hoverForce;
    }
    else
    {
      hoverForce = ctx.hoverForce * ctx.hoverForceFallingMultiplier;
    }

    ctx.rb.AddForce(hoverForce * Vector2.up);
  }

  private void LimitFallVelocity()
  {
    if (ctx.rb.velocity.y < ctx.maxFallVelocity)
    {
      ctx.rb.velocity = new Vector2(ctx.rb.velocity.x, ctx.maxFallVelocity);
    }
  }

  private void ApplyJumpCut()
  {
    if (ctx.jumpActionUp && ctx.rb.velocity.y > 0)
    {
      ctx.rb.AddForce(Vector2.down * ctx.rb.velocity.y * (1 - ctx.jumpCutMultiplier), ForceMode2D.Impulse);
    }
  }

  private void ApplyFallGravity()
  {
    // No gravity while in hovering state
    if (hovering)
    {
      ctx.rb.gravityScale = 0;
      return;
    }

    if (ctx.rb.velocity.y < 0)
    {
      ctx.rb.gravityScale = ctx.gravityScale * ctx.fallGravityMultiplier;
    }
    else
    {
      ctx.rb.gravityScale = ctx.gravityScale;
    }
  }

  // to avoid the scenario where we spam space in the air
  private void ResetNeedsToApplyJumpCut()
  {
    if (ctx.needsToApplyJumpCut)
    {
      counter += Time.deltaTime;
    }
    else
    {
      counter = 0f;
    }

    if (counter >= ctx.jumpBufferAmount)
    {
      ctx.needsToApplyJumpCut = false;
      counter = 0f;
    }
  }

  private IEnumerator DisableGroundCheckRoutine()
  {
    groundCheckEnabled = false;
    yield return new WaitForSeconds(0.2f);
    groundCheckEnabled = true;
  }
}