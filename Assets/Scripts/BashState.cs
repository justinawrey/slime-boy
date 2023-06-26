using UnityEngine;
using System.Collections.Generic;

public class BashState : BasePlayerAttackState
{

  // Keep track of all objects we've collided with while we're in our 
  // temporary bash state.  We don't want to collide with stuff twice. (YET!)
  private HashSet<int> instanceIds = new HashSet<int>();

  public BashState(PlayerAttackController ctx) : base(ctx) { }

  public override void Enter()
  {
    instanceIds.Clear();
  }

  public override void Update()
  {
    HandleBashCollisions();
  }

  private void HandleBashCollisions()
  {
    List<Collider2D> colliders = PlayerAttackController.GetBashColliders();
    foreach (Collider2D collider in colliders)
    {
      // If we've already seen the collider this bash state, ignore
      int id = GetInstanceId(collider);
      if (instanceIds.Contains(id))
      {
        continue;
      }

      // If its not damageable, ignore
      IDamageable damageable = collider.gameObject.GetComponent<IDamageable>();
      if (damageable == null)
      {
        continue;
      }

      // Cache it and deal damage, baby
      instanceIds.Add(id);
      DealDamage(damageable);
      MaybeRefreshCloudStep();
    }
  }

  private void DealDamage(IDamageable damageable)
  {
    damageable.TakeHit(ctx.BashDamage);
  }

  private void MaybeRefreshCloudStep()
  {
    bool isAirborn = PlayerController.playerStates.currentState.GetType() == typeof(AirbornState);
    if (isAirborn)
    {
      PlayerController.HasCloudStep = true;
    }
  }

  private int GetInstanceId(Collider2D collider)
  {
    return collider.gameObject.GetInstanceID();
  }
}