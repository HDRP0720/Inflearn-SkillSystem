using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Entity))]
public class Stats : MonoBehaviour
{
  #region Variables
  [SerializeField]
  private Stat _hpStat;

  [SerializeField]
  private Stat _skillCostStat;

  [Space]
  [SerializeField]
  private StatOverride[] _statOverrides;

  private Stat[] _stats;
  #endregion

  public Entity Owner { get; private set; }
  public Stat HPStat { get; private set; }
  public Stat SKillCostStat { get; private set; }

  private void OnDestroy()
  {
    foreach (var stat in _stats)
    {
      Destroy(stat);
    }

    _stats = null;
  }

  private void OnGUI()
  {
    if (!Owner.IsPlayer) return;
    
    GUI.Box(new Rect(2f, 2f, 250f, 250f), string.Empty);
    GUI.Label(new Rect(4f, 2f, 100f, 30f), "Player Stat");
    
    var textRect = new Rect(4f, 22f, 200f, 30f);
    var plusButtonRect = new Rect(textRect.x + textRect.width, textRect.y, 20f, 20f);
    var minusButtonRect = plusButtonRect;
    minusButtonRect.x += 22f;

    foreach (var stat in _stats)
    {
      string defaultValueAsString = stat.IsPercentType
        ? $"{stat.DefaultValue * 100f:0.##;-0.##}%"
        : stat.DefaultValue.ToString("0.##;-0.##");

      string bonusValueAsString = stat.IsPercentType
        ? $"{stat.BonusValue * 100f:0.##;-0.##}%"
        : stat.BonusValue.ToString("0.##;-0.##");
      
      GUI.Label(textRect, $"{stat.GetDisplayName}: {defaultValueAsString} ({bonusValueAsString})");
      
      if (GUI.Button(plusButtonRect, "+"))
      {
        if (stat.IsPercentType)
          stat.DefaultValue += 0.01f;
        else
          stat.DefaultValue += 1f;
      }
      
      if (GUI.Button(minusButtonRect, "-"))
      {
        if (stat.IsPercentType)
          stat.DefaultValue -= 0.01f;
        else
          stat.DefaultValue -= 1f;
      }
      
      textRect.y += 22f;
      plusButtonRect.y = minusButtonRect.y = textRect.y;
    }
  }

  public void Setup(Entity entity)
  {
    Owner = entity;

    _stats = _statOverrides.Select(x => x.CreateStat()).ToArray();
    HPStat = _hpStat ? GetStat(_hpStat) : null;
  }

  public Stat GetStat(Stat stat)
  {
    Debug.Assert(stat != null, $"Stats::GetStat - stat은 null이 될 수 없습니다.");
    return _stats.FirstOrDefault(x => x.GetID == stat.GetID);
  }

  public bool TryGetStat(Stat stat, out Stat outStat)
  {
    Debug.Assert(stat != null, $"Stats::TryGetStat - stat은 null이 될 수 없습니다.");

    outStat = _stats.FirstOrDefault(x => x.GetID == stat.GetID);
    return outStat != null;
  }

  public float GetValue(Stat stat) => GetStat(stat).Value;

  public bool HasStat(Stat stat)
  {
    Debug.Assert(stat != null, $"Stats::HasStat - stat은 null이 될 수 없습니다.");
    return _stats.Any(x => x.GetID == stat.GetID);
  }
  
  
  public void SetDefaultValue(Stat stat, float value) => GetStat(stat).DefaultValue = value;
  public float GetDefaultValue(Stat stat) => GetStat(stat).DefaultValue;

  public void IncreaseDefaultValue(Stat stat, float value) => SetDefaultValue(stat, GetDefaultValue(stat) + value);

  public void SetBonusValue(Stat stat, object key, float value) => GetStat(stat).SetBonusValue(key, value);
  public void SetBonusValue(Stat stat, object key, object subKey, float value) => GetStat(stat).SetBonusValue(key, subKey, value);

  public float GetBonusValue(Stat stat) => GetStat(stat).BonusValue;
  public float GetBonusValue(Stat stat, object key) => GetStat(stat).GetBonusValue(key);
  public float GetBonusValue(Stat stat, object key, object subKey) => GetStat(stat).GetBonusValue(key, subKey);
    
  public void RemoveBonusValue(Stat stat, object key) => GetStat(stat).RemoveBonusValue(key);
  public void RemoveBonusValue(Stat stat, object key, object subKey) => GetStat(stat).RemoveBonusValue(key, subKey);

  public bool ContainsBonusValue(Stat stat, object key) => GetStat(stat).ContainsBonusValue(key);
  public bool ContainsBonusValue(Stat stat, object key, object subKey) => GetStat(stat).ContainsBonusValue(key, subKey);

#if UNITY_EDITOR
  [ContextMenu("LoadStats")]
  private void LoadStats()
  {
    // 리소스 경로를 수정합니다.
    var assetsPath = "Assets/@Resources/Stat";

    // AssetDatabase API를 사용하여 Stat 클래스를 가져옵니다.
    var guids = AssetDatabase.FindAssets("t:Stat", new[] { assetsPath });
    var stats = guids.Select(guid => AssetDatabase.LoadAssetAtPath<Stat>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();

    // 나머지 코드는 동일합니다.
    stats.OrderBy(x => x.GetID);
    _statOverrides = stats.Select(x => new StatOverride(x)).ToArray();
    
    //var stats = Resources.LoadAll<Stat>("Stat").OrderBy(x => x.GetID);
    //_statOverrides = stats.Select(x => new StatOverride(x)).ToArray();
  }
#endif
}
