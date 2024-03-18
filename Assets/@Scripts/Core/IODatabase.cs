using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "IODatabase")]
public class IODatabase : ScriptableObject
{
  [SerializeField] 
  private List<IdentifiedObject> data = new();

  public IReadOnlyList<IdentifiedObject> GetData => data;
  public int GetCount => data.Count;

  public IdentifiedObject this[int index] => data[index];

  private void SetID(IdentifiedObject target, int id)
  {
    var field = typeof(IdentifiedObject).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);

    if (field != null)
      field.SetValue(target, id);
    
#if UNITY_EDITOR
    EditorUtility.SetDirty(target);
#endif
  }

  private void ReorderData()
  {
    var field = typeof(IdentifiedObject).GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
    for (int i = 0; i < data.Count; i++)
    {
      field.SetValue(data[i], i);
      
#if UNITY_EDITOR
      EditorUtility.SetDirty(data[i]);
#endif
    }
  }

  public void Add(IdentifiedObject newData)
  {
    data.Add(newData);
    SetID(newData, data.Count - 1);
  }

  public void Remove(IdentifiedObject datum)
  {
    data.Remove(datum);
    ReorderData();
  }

  public IdentifiedObject GetDataByID(int id) => data[id];
  public T GetDataByID<T>(int id) where T : IdentifiedObject => GetDataByID(id) as T;
  
  public IdentifiedObject GetDataCodeName(string codeName) => data.Find(item => item.GetCodeName == codeName);
  public T GetDataCodeName<T>(string codeName) where T : IdentifiedObject => GetDataCodeName(codeName) as T;

  public bool Contains(IdentifiedObject item) => data.Contains(item);

  public void SortByCodeName()
  {
    data.Sort((x, y) => x.GetCodeName.CompareTo(y.GetCodeName));
    ReorderData();
  }
}
