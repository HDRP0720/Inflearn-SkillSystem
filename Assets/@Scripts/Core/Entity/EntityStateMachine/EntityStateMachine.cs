using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStateMachine : MonoStateMachine<Entity>
{
  protected override void AddStates()
  {
    AddState<EntityDefaultState>();
    AddState<RollingState>();
    AddState<DeadState>();
  }

  protected override void MakeTransitions()
  {
    // Default State
    MakeTransition<EntityDefaultState, RollingState>(state => Owner.Movement?.IsRolling ?? false);

    // Rolling State
    MakeTransition<RollingState, EntityDefaultState>(state => !Owner.Movement.IsRolling);

    // Dead State
    MakeTransition<DeadState, EntityDefaultState>(state => !Owner.IsDead);

    MakeAnyTransition<EntityDefaultState>(EEntityStateCommand.ToDefaultState);
    MakeAnyTransition<DeadState>(state => Owner.IsDead);
  }
}
