using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoStateMachine<EntityType> : MonoBehaviour
{
  #region Event
  public delegate void StateChangedHandler(StateMachine<EntityType> stateMachine,
    State<EntityType> newState,
    State<EntityType> prevState,
    int layer);
  
  public event StateChangedHandler onStateChanged;
  #endregion
  
  private readonly StateMachine<EntityType> _stateMachine = new();
  
  public EntityType Owner => _stateMachine.Owner;
  
  private void Update()
  {
    if (Owner != null)
      _stateMachine.Update();
  }
  
  public void Setup(EntityType owner)
  {
    _stateMachine.Setup(owner);

    AddStates();
    MakeTransitions();
    _stateMachine.SetupLayers();

    _stateMachine.onStateChanged += (_, newState, prevState, layer)
      => onStateChanged?.Invoke(_stateMachine, newState, prevState, layer);
  }

  public void AddState<T>(int layer = 0) where T : State<EntityType>
  {
    _stateMachine.AddState<T>(layer);
  }

  #region MakeTransition Function Overloading
  public void MakeTransition<FromStateType, ToStateType>(int transitionCommand,
    Func<State<EntityType>, bool> transitionCondition,
    int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => _stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, transitionCondition, layer);

  public void MakeTransition<FromStateType, ToStateType>(Enum transitionCommand,
    Func<State<EntityType>, bool> transitionCondition,
    int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => _stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, transitionCondition, layer);
  
  public void MakeTransition<FromStateType, ToStateType>(
    Func<State<EntityType>, bool> transitionCondition, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => _stateMachine.MakeTransition<FromStateType, ToStateType>(int.MinValue, transitionCondition, layer);

  public void MakeTransition<FromStateType, ToStateType>(int transitionCommand, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => _stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, null, layer);
  
  public void MakeTransition<FromStateType, ToStateType>(Enum transitionCommand, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => _stateMachine.MakeTransition<FromStateType, ToStateType>(transitionCommand, null, layer);
  #endregion

  #region MakeAnyTransition Function Overloading
  public void MakeAnyTransition<ToStateType>(int transitionCommand,
    Func<State<EntityType>, bool> transitionCondition, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => _stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, transitionCondition, layer, canTransitionToSelf);

  public void MakeAnyTransition<ToStateType>(Enum transitionCommand,
    Func<State<EntityType>, bool> transitionCondition, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => _stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, transitionCondition, layer, canTransitionToSelf);

  public void MakeAnyTransition<ToStateType>(Func<State<EntityType>, bool> transitionCondition,
    int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => _stateMachine.MakeAnyTransition<ToStateType>(int.MinValue, transitionCondition, layer, canTransitionToSelf);

  public void MakeAnyTransition<ToStateType>(Enum transitionCommand, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => _stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, null, layer, canTransitionToSelf);

  public void MakeAnyTransition<ToStateType>(int transitionCommand, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => _stateMachine.MakeAnyTransition<ToStateType>(transitionCommand, null, layer, canTransitionToSelf);
  #endregion

  #region ExecuteCommand Function Overloading
  public bool ExecuteCommand(int transitionCommand, int layer)
    => _stateMachine.ExecuteCommand(transitionCommand, layer);

  public bool ExecuteCommand(Enum transitionCommand, int layer)
    => _stateMachine.ExecuteCommand(transitionCommand, layer);

  public bool ExecuteCommand(int transitionCommand)
    => _stateMachine.ExecuteCommand(transitionCommand);

  public bool ExecuteCommand(Enum transitionCommand)
    => _stateMachine.ExecuteCommand(transitionCommand);
  #endregion

  #region SendMessage Function Overloading
  public bool SendMessage(int message, int layer, object extraData = null)
    => _stateMachine.SendMessage(message, layer, extraData);

  public bool SendMessage(Enum message, int layer, object extraData = null)
    => _stateMachine.SendMessage(message, layer, extraData);

  public bool SendMessage(int message, object extraData = null)
    => _stateMachine.SendMessage(message, extraData);

  public bool SendMessage(Enum message, object extraData = null)
    => _stateMachine.SendMessage(message, extraData);
  #endregion
  
  public bool IsInState<T>() where T : State<EntityType>
    => _stateMachine.IsInState<T>();

  public bool IsInState<T>(int layer = 0) where T : State<EntityType>
    => _stateMachine.IsInState<T>(layer);

  public State<EntityType> GetCurrentState(int layer = 0) => _stateMachine.GetCurrentState(layer);

  public Type GetCurrentStateType(int layer = 0) => _stateMachine.GetCurrentStateType(layer);
  
  protected abstract void AddStates();
  protected abstract void MakeTransitions();
}
