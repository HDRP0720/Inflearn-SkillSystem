using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EEntityControlType { Player, AI }

public class Entity : MonoBehaviour
{
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
  #endregion
  
  private void Awake()
  {
    Animator = GetComponent<Animator>();
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
  
  public bool IsPlayer => _controlType == EEntityControlType.Player;

  public bool HasCategory(Category category) => _categories.Any(x => x.GetID == category.GetID);
}
