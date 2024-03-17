using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(IdentifiedObject), true)]
public class IdentifiedObjectEditor : Editor
{
  private SerializedProperty _categoriesProperty;
  private SerializedProperty _iconProperty;
  private SerializedProperty _idProperty;
  private SerializedProperty _codeNameProperty;
  private SerializedProperty _displayNameProperty;
  private SerializedProperty _descriptionProperty;

  private ReorderableList _categories;

  private GUIStyle _textAreaStyle;

  private readonly Dictionary<string, bool> _isFoldoutExpandedByTitle = new();

  protected virtual void OnEnable()
  {
    GUIUtility.keyboardControl = 0;

    _categoriesProperty = serializedObject.FindProperty("_categories");
    _iconProperty = serializedObject.FindProperty("_icon");
    _idProperty = serializedObject.FindProperty("_id");
    _codeNameProperty = serializedObject.FindProperty("_codeName");
    _displayNameProperty = serializedObject.FindProperty("_displayName");
    _descriptionProperty = serializedObject.FindProperty("_description");

    _categories = new(serializedObject, _categoriesProperty);
    _categories.drawHeaderCallback = rect => EditorGUI.LabelField(rect, _categoriesProperty.displayName);
    _categories.drawElementCallback = (rect, index, isActive, isFocused) =>
    {
      rect = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);

      EditorGUI.PropertyField(rect, _categoriesProperty.GetArrayElementAtIndex(index), GUIContent.none);
    };
  }

  private void StyleSetup()
  {
    if (_textAreaStyle == null)
    {
      _textAreaStyle = new(EditorStyles.textArea);
      _textAreaStyle.wordWrap = true;
    }
  }

  protected bool DrawFoldoutTitle(string text)
  {
    return CustomEditorUtility.DrawFoldoutTitle(_isFoldoutExpandedByTitle, text);
  }

  public override void OnInspectorGUI()
  {
    StyleSetup();
    
    _categories.DoLayoutList();

    if (DrawFoldoutTitle("Information"))
    {
      EditorGUILayout.BeginHorizontal("HelpBox");
      {
        _iconProperty.objectReferenceValue = EditorGUILayout.ObjectField(
          GUIContent.none, 
          _iconProperty.objectReferenceValue,
          typeof(Sprite), 
          false, 
          GUILayout.Width(65)
        );

        EditorGUILayout.BeginVertical();
        {
          EditorGUILayout.BeginHorizontal();
          {
            GUI.enabled = false;
            EditorGUILayout.PrefixLabel("ID");
            EditorGUILayout.PropertyField(_idProperty, GUIContent.none);
            GUI.enabled = true;
          }
          EditorGUILayout.EndHorizontal();
          
          EditorGUI.BeginChangeCheck();
          var prevCodeName = _codeNameProperty.stringValue;
          EditorGUILayout.DelayedTextField(_codeNameProperty);
          
          if (EditorGUI.EndChangeCheck())
          {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var newName = $"{target.GetType().Name.ToUpper()}_{_codeNameProperty.stringValue}";

            serializedObject.ApplyModifiedProperties();

            var message = AssetDatabase.RenameAsset(assetPath, newName);
            if (string.IsNullOrEmpty(message))
              target.name = newName;
            else
              _codeNameProperty.stringValue = prevCodeName;
          }

          EditorGUILayout.PropertyField(_displayNameProperty);
        }
        EditorGUILayout.EndVertical();
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginVertical("HelpBox");
      {
        EditorGUILayout.LabelField("Description");
        _descriptionProperty.stringValue = EditorGUILayout.TextArea(_descriptionProperty.stringValue,
          _textAreaStyle, GUILayout.Height(60));
      }
      EditorGUILayout.EndVertical();
    }

    serializedObject.ApplyModifiedProperties();
  }
}
