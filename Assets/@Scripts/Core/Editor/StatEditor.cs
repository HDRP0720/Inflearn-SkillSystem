using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stat))]
public class StatEditor : IdentifiedObjectEditor
{
  private SerializedProperty _isPercentTypeProperty;
  private SerializedProperty _maxValueProperty;
  private SerializedProperty _minValueProperty;
  private SerializedProperty _defaultValueProperty;

  protected override void OnEnable()
  {
    base.OnEnable();

    _isPercentTypeProperty = serializedObject.FindProperty("_isPercentType");
    _maxValueProperty = serializedObject.FindProperty("_maxValue");
    _minValueProperty = serializedObject.FindProperty("_minValue");
    _defaultValueProperty = serializedObject.FindProperty("_defaultValue");
  }

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();
    
    serializedObject.Update();

    if (DrawFoldoutTitle("Setting"))
    {
      EditorGUILayout.PropertyField(_isPercentTypeProperty);
      EditorGUILayout.PropertyField(_maxValueProperty);
      EditorGUILayout.PropertyField(_minValueProperty);
      EditorGUILayout.PropertyField(_defaultValueProperty);
    }

    serializedObject.ApplyModifiedProperties();
  }
}
