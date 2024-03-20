using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializeReferenceTest : MonoBehaviour
{
  [SerializeReference, SubclassSelector]
  private BaseClass _baseClass = new Test1();

  [SerializeField]
  private Test1 _test1;

  [ContextMenu("AllowTest1")]
  private void AllowTest1()
  {
    _baseClass = new Test1();
  }
  
  [ContextMenu("AllowTest2")]
  private void AllowTest2()
  {
    _baseClass = new Test2();
  }
}

[System.Serializable]
public abstract class BaseClass
{
  
}
[System.Serializable]
public class Test1 : BaseClass
{
  public int num;
}
[System.Serializable]
public class Test2 : BaseClass
{
  public Vector2 vector;
}