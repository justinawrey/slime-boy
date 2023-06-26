public class DebugStateChanges : StateObserver
{
  public bool showAttackStates = false;
  public bool showMovementSubstates = false;

  public override void OnAttackStateChanged(PlayerAttackStates playerAttackStates)
  {
    if (!showAttackStates)
    {
      return;
    }

    print("Attack state: " + playerAttackStates.currentState);
  }

  public override void OnSubstateChanged(PlayerStates playerStates)
  {
    if (!showMovementSubstates)
    {
      return;
    }

    print("Movement substate: " + playerStates.currentSubstate);
  }
}
