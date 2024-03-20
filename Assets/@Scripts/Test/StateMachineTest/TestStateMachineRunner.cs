using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStateMachineRunner : MonoBehaviour
{
  private TestStateMachine _stateMachine;

  private void Start()
  {
    _stateMachine = new();
    _stateMachine.Setup(gameObject);
  }

  private void Update()
  {
    _stateMachine.Update();
  }
}
