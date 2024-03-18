using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class EntityBaseLayerBehaviour : StateMachineBehaviour
{
  private Entity _entity;
  private NavMeshAgent _agent;
  private EntityMovement _movement;
  
  private static readonly int SpeedHash = Animator.StringToHash("speed");
  private static readonly int IsRollingHash = Animator.StringToHash("isRolling");
  private static readonly int IsDeadHash = Animator.StringToHash("isDead");

  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    if (_entity != null) return;
    
    _entity = animator.GetComponent<Entity>();
    _agent = animator.GetComponent<NavMeshAgent>();
    _movement = animator.GetComponent<EntityMovement>();
  }

  public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    if(_agent)
      animator.SetFloat(SpeedHash, _agent.desiredVelocity.sqrMagnitude / (_agent.speed * _agent.speed));
    
    if(_movement)
      animator.SetBool(IsRollingHash, _movement.IsRolling);
    
    animator.SetBool(IsDeadHash, _entity.IsDead);
  }
}
