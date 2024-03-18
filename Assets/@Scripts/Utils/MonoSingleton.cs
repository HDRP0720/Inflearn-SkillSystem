using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T _instance;
  private static bool _isQuitting;

  public static T Instance
  {
    get
    {
      if(_instance == null && !_isQuitting)
        _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include) ?? new GameObject(typeof(T).Name).AddComponent<T>();

      return _instance;
    }
  }
  
  protected virtual void OnApplicationQuit() => _isQuitting = true;
}
