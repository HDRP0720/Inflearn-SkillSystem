using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class IdentifiedObject : ScriptableObject, ICloneable
{
  #region Variables
  [SerializeField]
  private Category[] _categories;
  
  [SerializeField]
  private Sprite _icon;
  [SerializeField]
  private int _id = -1;
  [SerializeField][Tooltip("프로젝트에서 사용되는 파일명 입니다.")]
  private string _codeName;
  [SerializeField]
  private string _displayName;
  [SerializeField]
  private string _description;
  #endregion

  #region Properties
  public Sprite GetIcon => _icon;
  public int GetID => _id;
  public string GetCodeName => _codeName;
  public string GetDisplayName => _displayName;
  public virtual string GetDescription => _description;
  #endregion

  public object Clone() => Instantiate(this);
  
  public bool HasCategory(Category category) => _categories.Any(x => x.GetID == category.GetID);
  
  public bool HasCategory(string category) => _categories.Any(x => x == category);
}
