using UnityEngine;

[System.Serializable]
public class StatOverride
{
  [SerializeField]
  private Stat _stat;
  
  [SerializeField]
  private bool _isUseOverride;

  [SerializeField]
  private float _overrideDefaultValue;

  public StatOverride(Stat stat) => this._stat = stat;

  public Stat CreateStat()
  {
    var newStat = _stat.Clone() as Stat;
    if (_isUseOverride)
      newStat.DefaultValue = _overrideDefaultValue;
    
    return newStat;
  }

}
