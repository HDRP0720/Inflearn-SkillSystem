using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingState : State<Entity>
{
  private PlayerController _playerController;
  
  protected override void Setup()
  {
    _playerController = Entity.GetComponent<PlayerController>();
  }
  
  public override void Enter()
  {
    if (_playerController)
      _playerController.enabled = false;
  }
  
  public override void Exit()
  {
    if (_playerController)
      _playerController.enabled = true;
  }
}
