using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackStates
{
  public BasePlayerAttackState currentState;
  public BasePlayerAttackState prevState;
}

public class PlayerAttackController : MonoBehaviour
{
  public static event Action<PlayerAttackStates> AttackStateChanged;

  // States
  public static PlayerAttackStates playerAttackStates { private set; get; }
  public static NoAttackState noAttackState { private set; get; }
  public static BashState bashState { private set; get; }

  // Components
  public Rigidbody2D rb;

  // Input
  public bool bashActionDown { private set; get; }
  public Vector2 rawAimDir { private set; get; }

  // Parameters
  [Header("Parameters")]
  public float BashDamage = 1f;

  // Exposed static members
  [HideInInspector] public static Vector2 CurrPos;
  [HideInInspector] public static Dir CurrAimDir;

  private void Start()
  {
    rb = GetComponent<Rigidbody2D>();
    InitState();
  }

  public static void ForceCallback()
  {
    AttackStateChanged?.Invoke(PlayerAttackController.playerAttackStates);
  }

  private void InitState()
  {
    playerAttackStates = new PlayerAttackStates();
    noAttackState = new NoAttackState(this);
    bashState = new BashState(this);

    playerAttackStates.currentState = noAttackState;
    playerAttackStates.prevState = noAttackState;
  }

  private void SetInputs()
  {
    InputAction moveAction = ActionMapManager.FindActionFromMap(ActionMapManager.PlayerActionMapName, "Move");
    InputAction bashAction = ActionMapManager.FindActionFromMap(ActionMapManager.PlayerActionMapName, "Bash");

    rawAimDir = moveAction.ReadValue<Vector2>();
    bashActionDown = bashAction.WasPressedThisFrame();
  }

  private void Update()
  {
    SetInputs();

    CurrPos = transform.position;
    CurrAimDir = GetCurrAimDir();

    playerAttackStates.currentState.Update();
  }

  private void FixedUpdate()
  {
    playerAttackStates.currentState.FixedUpdate();
  }

  private Dir GetCurrAimDir()
  {
    float absX = Mathf.Abs(rawAimDir.x);
    float absY = Mathf.Abs(rawAimDir.y);

    if (absY > absX)
    {
      // Vertical aiming only takes precedence over facing direction
      // when the y vector is great than the x (i.e. we are "closest" to a vertical
      // cardinal direction)
      return Mathf.Sign(rawAimDir.y) > 0 ? Dir.UP : Dir.DOWN;
    }
    else
    {
      return PlayerController.FacingRight ? Dir.RIGHT : Dir.LEFT;
    }
  }

  public void SetAttackState(BasePlayerAttackState state)
  {
    if (state.GetType() == playerAttackStates.currentState.GetType())
    {
      return;
    }

    playerAttackStates.prevState = playerAttackStates.currentState;
    playerAttackStates.currentState = state;
    playerAttackStates.currentState.Enter();

    AttackStateChanged?.Invoke(playerAttackStates);
  }

  public static List<Collider2D> GetBashColliders()
  {
    Vector2 offset = GetBashColliderOffset();
    Vector2 size = GetBashColliderSize();

    Collider2D[] colliders = Physics2D.OverlapBoxAll(CurrPos + offset, size, 0);

    // Filter out the player.  It will always overlap due to the bash collider having a nice big hitbox.
    List<Collider2D> filteredColliders = new List<Collider2D>();
    foreach (Collider2D collider in colliders)
    {
      if (!collider.gameObject.CompareTag("Player"))
      {
        filteredColliders.Add(collider);
      }
    }

    return filteredColliders;
  }

  private static Vector2 GetBashColliderOffset()
  {
    switch (CurrAimDir)
    {
      case Dir.UP:
        return new Vector2(0.01f, 0.86f);
      case Dir.RIGHT:
        return new Vector2(0.54f, 0.33f);
      case Dir.LEFT:
        return new Vector2(-0.54f, 0.33f);
      case Dir.DOWN:
      // TODO;
      default:
        return new Vector2(0, 0);
    }
  }

  private static Vector2 GetBashColliderSize()
  {
    switch (CurrAimDir)
    {
      case Dir.UP:
        return new Vector2(0.4f, 0.68f);
      case Dir.RIGHT:
      case Dir.LEFT:
        return new Vector2(1f, 0.6f);
      case Dir.DOWN:
      // TODO;
      default:
        return new Vector2(0, 0);
    }
  }
}