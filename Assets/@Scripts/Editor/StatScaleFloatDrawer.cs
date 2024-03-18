using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatScaleFloat))]
public class StatScaleFloatDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    EditorGUI.BeginProperty(position, label, property);
    {
      var defaultValueProperty = property.FindPropertyRelative("defaultValue");
      var scaleStatProperty = property.FindPropertyRelative("scaleStat");
      
      position = EditorGUI.PrefixLabel(position, label);
      
      float adjust = EditorGUI.indentLevel * 15f;
      float halfWidth = (position.width * 0.5f) + adjust;
      
      var defaultValueRect = new Rect(position.x - adjust, position.y, halfWidth - 2.5f, position.height);
      defaultValueProperty.floatValue = EditorGUI.FloatField(defaultValueRect, GUIContent.none, defaultValueProperty.floatValue);

      var scaleStatRect = new Rect(defaultValueRect.x + defaultValueRect.width - adjust + 2.5f, position.y, halfWidth, position.height);
      scaleStatProperty.objectReferenceValue = EditorGUI.ObjectField(scaleStatRect, GUIContent.none, scaleStatProperty.objectReferenceValue, typeof(Stat), false);
    }
    EditorGUI.EndProperty();
   
    
    
  }
}
