using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State<Entity>
{
  private PlayerController _playerController;
  private EntityMovement _movement;
  
  protected override void Setup()
  {
    _playerController = Entity.GetComponent<PlayerController>();
    _movement = Entity.GetComponent<EntityMovement>();
  }
  
  public override void Enter()
  {
    if (_playerController)
      _playerController.enabled = false;

    if (_movement)
      _movement.enabled = false;
  }
  
  public override void Exit()
  {
    if (_playerController)
      _playerController.enabled = true;

    if (_movement)
      _movement.enabled = true;
  }
}
