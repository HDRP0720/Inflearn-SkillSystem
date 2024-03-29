using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EEntityControlType { Player, AI }

public class Entity : MonoBehaviour
{
  public delegate void TakeDamageHandler(Entity entity, Entity instigator, object causer, float damage);
  public delegate void DeadHandler(Entity entity);
  
  public event TakeDamageHandler onTakeDamage;
  public event DeadHandler onDead;
  
  #region Variables
  [SerializeField] 
  private Category[] _categories;

  [SerializeField] 
  private EEntityControlType _controlType;

  private Dictionary<string, Transform> _socketsByName = new();
  #endregion

  #region Properties
  public EEntityControlType GetControlType => _controlType;
  public IReadOnlyList<Category> GetCategories => _categories;
  public Animator Animator { get; private set; }
  public Stats Stats { get; private set; }
  public EntityMovement Movement { get; private set; }
  public MonoStateMachine<Entity> StateMachine { get; private set; }
  public Entity Target { get; set; }
  #endregion
  
  private void Awake()
  {
    Animator = GetComponent<Animator>();
    
    Stats = GetComponent<Stats>();
    Stats.Setup(this);

    Movement = GetComponent<EntityMovement>();
    if (Movement != null)
      Movement.Setup(this);

    StateMachine = GetComponent<MonoStateMachine<Entity>>();
    if (StateMachine != null)
      StateMachine.Setup(this);
  }
  
  public Transform GetTransformSocket(string socketName)
  {
    if (_socketsByName.TryGetValue(socketName, out var socket))
      return socket;

    socket = GetTransformSocket(transform, socketName);
    if (socket)
      _socketsByName[socketName] = socket;

    return socket;
  }
  private Transform GetTransformSocket(Transform root, string socketName)
  {
    if(root.name == socketName)
      return root;

    foreach (Transform child in root)
    {
      var socket = GetTransformSocket(child, socketName);
      if (socket)
        return socket;
    }

    return null;
  }

  public void TakeDamage(Entity instigator, object causer, float damage)
  {
    if (IsDead) return;

    float prevValue = Stats.HPStat.DefaultValue;
    Stats.HPStat.DefaultValue -= damage;
    
    onTakeDamage?.Invoke(this, instigator, causer, damage);

    if (Mathf.Approximately(Stats.HPStat.DefaultValue, 0f))
      OnDead();
  }

  private void OnDead()
  {
    if (Movement != null)
      Movement.enabled = false;
    
    onDead?.Invoke(this);
  }
  
  public bool IsPlayer => _controlType == EEntityControlType.Player;

  public bool IsDead => Stats.HPStat != null && Mathf.Approximately(Stats.HPStat.DefaultValue, 0f);

  public bool HasCategory(Category category) => _categories.Any(x => x.GetID == category.GetID);

  public bool IsInState<T>() where T : State<Entity> => StateMachine.IsInState<T>();
  public bool IsInState<T>(int layer) where T : State<Entity> => StateMachine.IsInState<T>(layer);
}
