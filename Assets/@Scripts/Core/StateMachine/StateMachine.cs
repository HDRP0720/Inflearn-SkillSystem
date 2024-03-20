using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine<EntityType>
{
  private class StateData
  {
    public int Layer { get; private set; }
    public int Priority { get; private set; }
    public State<EntityType> State { get; private set; }
    public List<StateTransition<EntityType>> Transitions { get; private set; } = new();
    
    // Constructor
    public StateData(int layer, int priority, State<EntityType> state)
      => (Layer, Priority, State) = (layer, priority, state);
  }
  
  public delegate void StateChangedHandler(StateMachine<EntityType> stateMachine,
    State<EntityType> newState, State<EntityType> prevState, int layer);
  
  public event StateChangedHandler onStateChanged;
  
  private readonly Dictionary<int, Dictionary<Type, StateData>> _stateDataByLayer = new();
  private readonly Dictionary<int, List<StateTransition<EntityType>>> _anyTransitionsByLayer = new();
  private readonly Dictionary<int, StateData> _currentStateDataByLayer = new();
  
  private readonly SortedSet<int> _layers = new();
  
  public EntityType Owner { get; private set; }

  public void Setup(EntityType owner)
  {
    Debug.Assert(owner != null, $"StateMachine<{typeof(EntityType).Name}>::Setup - owner는 null이 될 수 없습니다.");

    Owner = owner;

    AddStates();
    MakeTransitions();
    SetupLayers();
  }
  
  public void SetupLayers()
  {
    foreach ((int layer, var statDatasByType) in _stateDataByLayer)
    {
      _currentStateDataByLayer[layer] = null;
 
      var firstStateData = statDatasByType.Values.First(x => x.Priority == 0);
      ChangeState(firstStateData);
    }
  }
  
  private void ChangeState(StateData newStateData)
  {
    var prevState = _currentStateDataByLayer[newStateData.Layer];
    prevState?.State.Exit();

    _currentStateDataByLayer[newStateData.Layer] = newStateData;
    newStateData.State.Enter();

    onStateChanged?.Invoke(this, newStateData.State, prevState.State, newStateData.Layer);
  }
  private void ChangeState(State<EntityType> newState, int layer)
  {
    var newStateData = _stateDataByLayer[layer][newState.GetType()];
    ChangeState(newStateData);
  }

  private bool TryTransition(IReadOnlyList<StateTransition<EntityType>> transitions, int layer)
  {
    foreach (var transition in transitions)
    {
      if (transition.TransitionCommand != StateTransition<EntityType>.kNullCommand || !transition.IsTransferable)
        continue;
      
      if (!transition.CanTransitionToSelf && _currentStateDataByLayer[layer].State == transition.ToState)
        continue;
      
      ChangeState(transition.ToState, layer);
      return true;
    }

    return false;
  }

  public void Update()
  {
    foreach (var layer in _layers)
    {
      var currentStateDatum = _currentStateDataByLayer[layer];
      
      bool hasAnyTransitions = _anyTransitionsByLayer.TryGetValue(layer, out var anyTransitions);
      
      if ((hasAnyTransitions && TryTransition(anyTransitions, layer)) ||
          TryTransition(currentStateDatum.Transitions, layer))
        continue;
      
      currentStateDatum.State.Update();
    }
  }

  public void AddState<T>(int layer = 0) where T : State<EntityType>
  {
    _layers.Add(layer);

    var newState = Activator.CreateInstance<T>();
    newState.Setup(this, Owner, layer);

    if (!_stateDataByLayer.ContainsKey(layer))
    {
      _stateDataByLayer[layer] = new();
      
      _anyTransitionsByLayer[layer] = new();
    }
    
    Debug.Assert(!_stateDataByLayer[layer].ContainsKey(typeof(T)),
      $"StateMachine::AddState<{typeof(T).Name}> - 이미 상태가 존재합니다.");
    
    var stateDataByType = _stateDataByLayer[layer];
    stateDataByType[typeof(T)] = new StateData(layer, stateDataByType.Count, newState);
  }

  public void MakeTransition<FromStateType, ToStateType>(int transitionCommand,
    Func<State<EntityType>, bool> transitionCondition, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
  {
    var stateData = _stateDataByLayer[layer];
    var fromStateData = stateData[typeof(FromStateType)];
    var toStateData = stateData[typeof(ToStateType)];
    
    var newTransition = new StateTransition<EntityType>(fromStateData.State, toStateData.State,
      transitionCommand, transitionCondition, true);
    
    fromStateData.Transitions.Add(newTransition);
  }
  public void MakeTransition<FromStateType, ToStateType>(Enum transitionCommand,
    Func<State<EntityType>, bool> transitionCondition, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => MakeTransition<FromStateType, ToStateType>(Convert.ToInt32(transitionCommand), transitionCondition, layer);
  public void MakeTransition<FromStateType, ToStateType>(Func<State<EntityType>, bool> transitionCondition, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => MakeTransition<FromStateType, ToStateType>(StateTransition<EntityType>.kNullCommand, transitionCondition, layer);
  public void MakeTransition<FromStateType, ToStateType>(int transitionCommand, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => MakeTransition<FromStateType, ToStateType>(transitionCommand, null, layer);
  public void MakeTransition<FromStateType, ToStateType>(Enum transitionCommand, int layer = 0)
    where FromStateType : State<EntityType>
    where ToStateType : State<EntityType>
    => MakeTransition<FromStateType, ToStateType>(transitionCommand, null, layer);

  public void MakeAnyTransition<ToStateType>(int transitionCommand,
    Func<State<EntityType>, bool> transitionCondition, int layer = 0, bool canTransitonToSelf = false)
    where ToStateType : State<EntityType>
  {
    var stateDataByType = _stateDataByLayer[layer];
    var state = stateDataByType[typeof(ToStateType)].State;
    var newTransition = new StateTransition<EntityType>(null, state, transitionCommand, transitionCondition, canTransitonToSelf);
    
    _anyTransitionsByLayer[layer].Add(newTransition);
  }
  public void MakeAnyTransition<ToStateType>(Enum transitionCommand,
    Func<State<EntityType>, bool> transitionCondition, int layer = 0, bool canTransitonToSelf = false)
    where ToStateType : State<EntityType>
    => MakeAnyTransition<ToStateType>(Convert.ToInt32(transitionCommand), transitionCondition, layer, canTransitonToSelf);
  public void MakeAnyTransition<ToStateType>(Func<State<EntityType>, bool> transitionCondition,
    int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => MakeAnyTransition<ToStateType>(StateTransition<EntityType>.kNullCommand, transitionCondition, layer, canTransitionToSelf);
  public void MakeAnyTransition<ToStateType>(int transitionCommand, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => MakeAnyTransition<ToStateType>(transitionCommand, null, layer, canTransitionToSelf);
  public void MakeAnyTransition<ToStateType>(Enum transitionCommand, int layer = 0, bool canTransitionToSelf = false)
    where ToStateType : State<EntityType>
    => MakeAnyTransition<ToStateType>(transitionCommand, null, layer, canTransitionToSelf);

  public bool ExecuteCommand(int transitionCommand, int layer)
  {
    var transition = _anyTransitionsByLayer[layer].Find(x =>
      x.TransitionCommand == transitionCommand && x.IsTransferable);
    
    transition ??= _currentStateDataByLayer[layer].Transitions.Find(x =>
      x.TransitionCommand == transitionCommand && x.IsTransferable);
    
    if (transition == null)
      return false;
    
    ChangeState(transition.ToState, layer);
    return true;
  }
  public bool ExecuteCommand(Enum transitionCommand, int layer)
    => ExecuteCommand(Convert.ToInt32(transitionCommand), layer);
  public bool ExecuteCommand(int transitionCommand)
  {
    bool isSuccess = false;
    foreach (var layer in _layers)
    {
      if (ExecuteCommand(transitionCommand, layer))
        isSuccess = true;
    }

    return isSuccess;
  }
  public bool ExecuteCommand(Enum transitionCommand)
    => ExecuteCommand(Convert.ToInt32(transitionCommand));
  
  public bool SendMessage(int message, int layer, object extraData = null)
    => _currentStateDataByLayer[layer].State.OnReceiveMessage(message, extraData);
  public bool SendMessage(Enum message, int layer, object extraData = null)
    => SendMessage(Convert.ToInt32(message), layer, extraData);
  public bool SendMessage(int message, object extraData = null)
  {
    bool isSuccess = false;
    foreach (int layer in _layers)
    {
      if (SendMessage(message, layer, extraData))
        isSuccess = true;
    }
    return isSuccess;
  }
  public bool SendMessage(Enum message, object extraData = null)
    => SendMessage(Convert.ToInt32(message), extraData);
  
  public bool IsInState<T>() where T : State<EntityType>
  {
    foreach ((_, StateData data) in _currentStateDataByLayer)
    {
      if (data.State.GetType() == typeof(T))
        return true;
    }
    return false;
  }
  public bool IsInState<T>(int layer) where T : State<EntityType>
  {
    return _currentStateDataByLayer[layer].State.GetType() == typeof(T);
  }
  
  public Type GetCurrentStateType(int layer = 0) => GetCurrentState(layer).GetType();
  public State<EntityType> GetCurrentState(int layer = 0) => _currentStateDataByLayer[layer].State;
  
  protected virtual void AddStates() { }
  protected virtual void MakeTransitions() { }
}
