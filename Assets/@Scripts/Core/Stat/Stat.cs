using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Stat : IdentifiedObject
{
  public delegate void ValueChangedHandler(Stat stat, float currentValue, float prevValue);
  
  public event ValueChangedHandler onValueChanged;
  public event ValueChangedHandler onValueMax;
  public event ValueChangedHandler onValueMin;

  //public event Action<Stat, float, float> onValueChanged; delegate 와 action의 차이는 무엇인가?
  
  #region Variables
  [SerializeField] 
  private bool _isPercentType;

  [SerializeField] 
  private float _maxValue;

  [SerializeField] 
  private float _minValue;

  [SerializeField] 
  private float _defaultValue;

  private Dictionary<object, Dictionary<object, float>> _bonusValuesByKey = new();
  #endregion

  #region Properties
  public float MaxValue
  {
    get => _maxValue;
    set => _maxValue = value;
  }

  public float MinValue
  {
    get => _minValue;
    set => _minValue = value;
  }

  public float DefaultValue
  {
    get => _defaultValue;
    set
    {
      float prevValue = Value;
      _defaultValue = Mathf.Clamp(value, MinValue, MaxValue);
      TryInvokeValueChangedEvent(Value, prevValue);
    }
  }
  
  public float BonusValue { get; private set; }
  #endregion
  
  public bool IsPercentType => _isPercentType;
  public float Value => Mathf.Clamp(_defaultValue + BonusValue, MinValue, MaxValue);
  public bool IsMax => Mathf.Approximately(Value, _maxValue);
  public bool IsMin => Mathf.Approximately(Value, _minValue);

  private void TryInvokeValueChangedEvent(float currentValue, float prevValue)
  {
    if (!Mathf.Approximately(currentValue, prevValue))
    {
      onValueChanged?.Invoke(this, currentValue, prevValue);
      if(Mathf.Approximately(currentValue, MaxValue))
        onValueMax?.Invoke(this, MaxValue, prevValue);
      else if(Mathf.Approximately(currentValue, MinValue))
        onValueMin?.Invoke(this, MinValue, prevValue);
    }
  }
  
  public void SetBonusValue(object key, float value) => SetBonusValue(key, string.Empty, value);
  public void SetBonusValue(object key, object subKey, float value)
  {
    if (!_bonusValuesByKey.ContainsKey(key))
      _bonusValuesByKey[key] = new Dictionary<object, float>();
    else
      BonusValue -= _bonusValuesByKey[key][subKey];

    float prevValue = Value;
    _bonusValuesByKey[key][subKey] = value;
    BonusValue += value;

    TryInvokeValueChangedEvent(Value, prevValue);
  }

  public float GetBonusValue(object key)
  {
    if (_bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubKey))
      return bonusValuesBySubKey.Sum(x => x.Value);
    
    return 0f;
  }
  public float GetBonusValue(object key, object subKey)
  {
    if (_bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubKey))
    {
      if (bonusValuesBySubKey.TryGetValue(subKey, out var value))
        return value;
    }

    return 0f;
  }

  public bool RemoveBonusValue(object key)
  {
    if (_bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubKey))
    {
      float prevValue = Value;
      BonusValue -= bonusValuesBySubKey.Values.Sum();
      _bonusValuesByKey.Remove(key);
      
      TryInvokeValueChangedEvent(Value, prevValue);
      return true;
    }

    return false;
  }
  public bool RemoveBonusValue(object key, object subKey)
  {
    if (_bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubKey))
    {
      if (bonusValuesBySubKey.Remove(subKey, out var value))
      {
        float prevValue = Value;
        BonusValue -= value;
        TryInvokeValueChangedEvent(Value, prevValue);
        return true;
      }
    }

    return false;
  }

  public bool ContainsBonusValue(object key) => _bonusValuesByKey.ContainsKey(key);
  public bool ContainsBonusValue(object key, object subKey)
  {
    if (_bonusValuesByKey.TryGetValue(key, out var bonusValuesBySubKey))
      return bonusValuesBySubKey.ContainsKey(subKey);
    
    return false;
  }
}
