using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class CustomEditorUtility
{
  private static readonly GUIStyle _titleStyle;

  static CustomEditorUtility()
  {
    _titleStyle = new GUIStyle("ShurikenModuleTitle")
    {
      font = new GUIStyle(EditorStyles.label).font,
      fontStyle = FontStyle.Bold,
      fontSize = 14,
      border = new RectOffset(15, 7, 4, 4),
      fixedHeight = 26f,
      contentOffset = new Vector2(20f, -2f)
    };
  }

  public static bool DrawFoldoutTitle(string title, bool isExpanded, float space = 15f)
  {
    EditorGUILayout.Space(space);

    var rect = GUILayoutUtility.GetRect(16f, _titleStyle.fixedHeight, _titleStyle);
    
    GUI.Box(rect, title, _titleStyle);

    var currentEvent = Event.current;

    var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);

    if (currentEvent.type == EventType.Repaint)
    {
      EditorStyles.foldout.Draw(toggleRect, false, false, isExpanded, false);
    }
    else if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
    {
      isExpanded = !isExpanded;
      
      currentEvent.Use();
    }
    
    return isExpanded;
  }

  public static bool DrawFoldoutTitle(IDictionary<string, bool> isFoldoutExpandedByTitle, string title, float space = 15f)
  {
    if(!isFoldoutExpandedByTitle.ContainsKey(title))
      isFoldoutExpandedByTitle[title] = true;

    isFoldoutExpandedByTitle[title] = DrawFoldoutTitle(title, isFoldoutExpandedByTitle[title], space);

    return isFoldoutExpandedByTitle[title];
  }

  public static void DrawUnderline(float height = 1f)
  {
    var lastRect = GUILayoutUtility.GetLastRect();

    lastRect.y += lastRect.height;
    lastRect.height = height;
    
    EditorGUI.DrawRect(lastRect, Color.gray);
  }
}
