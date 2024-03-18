using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ECursorType { Default, BlueArrow }
public class MouseController : MonoSingleton<MouseController>
{
  [System.Serializable]
  private struct CursorData
  {
    public ECursorType type;
    public Texture2D texture;
  }
  
  public delegate void ClickedHandler(Vector2 mousePosition);
  
  public event ClickedHandler onLeftClicked;
  public event ClickedHandler onRightClicked;
  
  [SerializeField]
  private CursorData[] _cursorData;

  private void Update()
  {
    if(Input.GetMouseButtonDown(0))
      onLeftClicked?.Invoke(Input.mousePosition);
    else if(Input.GetMouseButtonDown(1))
      onRightClicked?.Invoke(Input.mousePosition);
  }

  public void ChangeCursor(ECursorType newType)
  {
    if(newType == ECursorType.Default)
      Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    else
    {
      var cursorTexture = _cursorData.First(x => x.type == newType).texture;
      Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
  }
}
