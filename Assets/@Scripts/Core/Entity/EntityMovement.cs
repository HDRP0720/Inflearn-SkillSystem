using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntityMovement : MonoBehaviour
{
  public delegate void SetDestinationHandler(EntityMovement movement, Vector3 destination);
  
  public event SetDestinationHandler onSetDestination;

  #region Variables
  [SerializeField]
  private Stat _moveSpeedStat;
  [SerializeField]
  private float _rollTime = 0.5f;
  
  private NavMeshAgent _agent;
  private Transform _traceTarget;
  private Stat _entityMoveSpeedStat;
  
  private Coroutine _coLookAt;
  private Coroutine _coTrace;
  private Coroutine _coRoll;
  
  // 구르기 애니메이션 배속을 위한 세팅 값
  private static readonly int RollSpeedHash = Animator.StringToHash("rollSpeed");
  #endregion

  #region Properties
  public Entity Owner { get; private set; }
  public float MoveSpeed => _agent.speed;
  public bool IsRolling { get; private set; }
  public Transform TraceTarget
  {
    get => _traceTarget;
    set
    {
      if (_traceTarget == value) return;
      
      Stop();

      _traceTarget = value;
      if (_traceTarget)
        StartCoroutine(CoTraceUpdate());
    }
  }
  public Vector3 Destination
  {
    get => _agent.destination;
    set
    {
      TraceTarget = null;
      SetDestination(value);
    }
  }
  #endregion

  #region Unity Functions
  private void OnDisable() => Stop();
  private void OnDestroy()
  {
    if (_entityMoveSpeedStat)
      _entityMoveSpeedStat.onValueChanged -= OnMoveSpeedChanged;
  }
  #endregion
  
  public void Setup(Entity owner)
  {
    Owner = owner;

    _agent = Owner.GetComponent<NavMeshAgent>();
    _agent.updateRotation = false;

    var animator = Owner.Animator;
    if(animator)
      animator.SetFloat(RollSpeedHash, 1 / _rollTime);

    _entityMoveSpeedStat = _moveSpeedStat ? Owner.Stats.GetStat(_moveSpeedStat) : null;
    if (_entityMoveSpeedStat)
    {
      _agent.speed = _entityMoveSpeedStat.Value;
      _entityMoveSpeedStat.onValueChanged += OnMoveSpeedChanged;
    }
  }
  
  private void SetDestination(Vector3 destination)
  {
    _agent.destination = destination;
    LookAt(destination);
    
    onSetDestination?.Invoke(this, destination);
  }
  
  public void Stop()
  {
    _traceTarget = null;

    if (_coTrace != null)
      StopCoroutine(_coTrace);
    
    if(_agent.isOnNavMesh)
      _agent.ResetPath();
    
    _agent.velocity = Vector3.zero;
  }

  public void LookAt(Vector3 position)
  {
    if (_coLookAt != null)
      StopCoroutine(_coLookAt);
 
    _coLookAt = StartCoroutine(CoLookAtUpdate(position));
  }
  public void LookAtImmediate(Vector3 position)
  {
    position.y = transform.position.y;
    var lookDirection = (position - transform.position).normalized;
    var rotation = lookDirection != Vector3.zero ? Quaternion.LookRotation(lookDirection) : transform.rotation;
    transform.rotation = rotation;
  }
  private IEnumerator CoLookAtUpdate(Vector3 position)
  {
    position.y = transform.position.y;
    var lookDirection = (position - transform.position).normalized;
    var rotation = lookDirection != Vector3.zero ? Quaternion.LookRotation(lookDirection) : transform.rotation;
    var speed = 180f / 0.15f; // 180도 회전에 0.15초로 설정 (일반적으로 0.1 ~ 0.3초가 적당 )

    while (true)
    {
      transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, speed * Time.deltaTime);
      if (transform.rotation == rotation)
        break;

      yield return null;
    }
  }

  public void Roll(float distance) => Roll(distance, transform.forward);
  public void Roll(float distance, Vector3 direction)
  {
    Stop();
    
    if(direction != Vector3.zero)
      transform.rotation = Quaternion.LookRotation(direction);

    IsRolling = true;
    
    if(_coRoll != null)
      StopCoroutine(_coRoll);

    _coRoll = StartCoroutine(CoRollUpdate(distance));
  }
  private IEnumerator CoRollUpdate(float rollDistance)
  {
    float currentRollTime = 0f;   // 현재까지 구른 시간
    float prevRollDistance = 0f;  // 이전 frame에 이동한 거리

    while (true)
    {
      currentRollTime += Time.deltaTime;

      // Easing InOutSine https://easings.net/ko#easeInOutSine
      // Easing 함수 계산
      float timePoint = currentRollTime / _rollTime;
      float inOutSine = -(Mathf.Cos(Mathf.PI * timePoint) - 1f) / 2f;
      float currentRollDistance = Mathf.Lerp(0f, rollDistance, inOutSine);
      
      float deltaValue = currentRollDistance - prevRollDistance;  // 이번 frame에 움직일 거리
      
      transform.position += (transform.forward * deltaValue);
      prevRollDistance = currentRollDistance;

      if (currentRollTime >= _rollTime)
        break;
      else
        yield return null;
    }

    IsRolling = false;
  }
  
  private IEnumerator CoTraceUpdate()
  {
    while (true)
    {
      if (Vector3.SqrMagnitude(TraceTarget.position - transform.position) > 1.0f)
      {
        SetDestination(TraceTarget.position);
        yield return null;
      }
      else
        break;
    }
  }
  
  private void OnMoveSpeedChanged(Stat stat, float currentValue, float prevValue)
    => _agent.speed = currentValue;
}
