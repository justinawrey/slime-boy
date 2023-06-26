using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerStates
{
  public BasePlayerState currentState;
  public BasePlayerState prevState;
  public string currentSubstate;
  public string prevSubstate;
}

public class PlayerController : MonoBehaviour
{
  public static event Action<PlayerStates> SubstateChanged;

  // States
  public static PlayerStates playerStates { private set; get; }
  public static GroundedState groundedState { private set; get; }
  public static AirbornState airbornState { private set; get; }

  // Input
  public float horizontalAxis { private set; get; }
  public bool jumpActionDown { private set; get; }
  public bool jumpActionUp { private set; get; }
  public bool hoverActionDown { private set; get; }
  public bool hoverActionUp { private set; get; }

  // Components
  public Rigidbody2D rb { private set; get; }
  public SpriteRenderer spriteRenderer { private set; get; }
  public BoxCollider2D boxCollider { private set; get; }

  // Leeway Timers
  public Leeway coyoteTime { private set; get; }
  public Leeway jumpBuffer { private set; get; }

  [Header("Movement Parameters")]
  public float maxVelocity = 2f;
  public float idleMovemovementThreshold = 0.5f;
  public float runAcceleration = 1.5f;
  public float runDeceleration = 1.5f;
  public float airAcceleration = 1.2f;
  public float airDeceleration = 1.2f;
  public float maxFallVelocity = -15f;
  public float friction = 0.2f;
  public float gravityScale = 1f;
  public float fallGravityMultiplier = 1.5f;
  public float jumpForce = 1f;
  public float jumpCutMultiplier = 0.1f;
  public float coyoteTimeAmount = 0.15f;
  public float jumpBufferAmount = 0.2f;
  public float hoverForce = 6f;
  public float hoverForceFallingMultiplier = 6f;
  public float maxUpwardsVelocityWhileHovering = 6f;

  [HideInInspector] public static bool FacingRight = true;
  [HideInInspector] public static bool HasCloudStep = false;
  [HideInInspector] public static int NumJumps = 0;

  [HideInInspector]
  public bool needsToApplyJumpCut = false;

  // Do all of this in Start and not Awake,
  // because this object will be persisted throughout scenes under the Globals object.
  // If we were to do this in Awake, the component references and state classes would be
  // created, but then the Globals object would destroy its clone and thus we would
  // have orphan scripts with no attached game object.  By initializing in start,
  // we only try to initialize when the duplicate object has already been destroyed
  private void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    boxCollider = GetComponent<BoxCollider2D>();

    coyoteTime = new Leeway(coyoteTimeAmount);
    jumpBuffer = new Leeway(jumpBufferAmount);

    InitState();
  }

  public static void ForceCallback()
  {
    SubstateChanged?.Invoke(PlayerController.playerStates);
  }

  private void InitState()
  {
    playerStates = new PlayerStates();
    groundedState = new GroundedState(this);
    airbornState = new AirbornState(this);

    playerStates.currentState = groundedState;
    playerStates.prevState = groundedState;
    playerStates.currentSubstate = GroundedState.IdleSubState;
    playerStates.prevSubstate = GroundedState.IdleSubState;
  }

  private void SetInputs()
  {
    InputAction moveAction = ActionMapManager.FindActionFromMap(ActionMapManager.PlayerActionMapName, "Move");
    InputAction jumpAction = ActionMapManager.FindActionFromMap(ActionMapManager.PlayerActionMapName, "Jump");
    InputAction hoverAction = ActionMapManager.FindActionFromMap(ActionMapManager.PlayerActionMapName, "Hover");

    horizontalAxis = moveAction.ReadValue<Vector2>().x;
    jumpActionDown = jumpAction.WasPressedThisFrame();
    jumpActionUp = jumpAction.WasReleasedThisFrame();
    hoverActionDown = hoverAction.IsPressed();
    hoverActionUp = hoverAction.WasReleasedThisFrame();
  }

  private void FlipSprite()
  {
    // Can't flip sprite while bashing, but you can still do normal physics
    if (PlayerAttackController.playerAttackStates.currentState.GetType() == typeof(BashState))
    {
      return;
    }

    if (horizontalAxis > 0)
    {
      if (!FacingRight)
      {
        transform.Rotate(new Vector3(0, 180, 0));
        FacingRight = true;
      }
    }
    else if (horizontalAxis < 0)
    {
      if (FacingRight)
      {
        transform.Rotate(new Vector3(0, -180, 0));
        FacingRight = false;
      }
    }
  }

  private void TickTimers()
  {
    if (playerStates.currentState == groundedState)
    {
      coyoteTime.Reset();
    }
    else
    {
      coyoteTime.Tick(Time.deltaTime);
    }

    if (jumpActionDown)
    {
      jumpBuffer.Reset();
    }
    else
    {
      jumpBuffer.Tick(Time.deltaTime);
    }
  }

  private void Update()
  {
    SetInputs();
    FlipSprite();
    TickTimers();

    // If space key up was released during the jump buffer duration, we need to apply
    // the jump cut still
    if (jumpBuffer.Valid() && jumpActionUp)
    {
      needsToApplyJumpCut = true;
    }

    playerStates.currentState.Update();
  }

  private void FixedUpdate()
  {
    playerStates.currentState.FixedUpdate();
  }

  public Collider2D GetGroundedCollider()
  {
    Vector3 offset = new Vector3(-0.05f, 0.01f, 0f);
    return Physics2D.OverlapCapsule(transform.position + offset, new Vector2(0.15f, 0.03f), CapsuleDirection2D.Horizontal, 0, LayerMask.GetMask("Ground"));
  }

  public bool CheckGround()
  {
    return GetGroundedCollider();
  }

  public void ApplyJumpForce(float force)
  {
    rb.velocity = new Vector2(rb.velocity.x, 0);

    // A "phantom jump" needs to be applied if the jump key was pressed AND released
    // during the jump buffer timer
    if (needsToApplyJumpCut)
    {
      rb.AddForce(Vector2.up * (force * (0.7f)), ForceMode2D.Impulse);
    }
    else
    {
      rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    coyoteTime.Invalidate();
    jumpBuffer.Invalidate();
    NumJumps++;
    HasCloudStep = false;
    needsToApplyJumpCut = false;
  }

  public void SetState(BasePlayerState state)
  {
    if (state.GetType() == playerStates.currentState.GetType())
    {
      return;
    }

    playerStates.prevState = playerStates.currentState;
    playerStates.currentState = state;
    playerStates.currentState.Enter();
  }

  public void SetSubstate(string substate)
  {
    if (substate == playerStates.currentSubstate)
    {
      return;
    }

    playerStates.prevSubstate = playerStates.currentSubstate;
    playerStates.currentSubstate = substate;
    SubstateChanged?.Invoke(playerStates);
  }
}