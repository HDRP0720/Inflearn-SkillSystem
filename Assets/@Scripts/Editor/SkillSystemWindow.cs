using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;


public class SkillSystemWindow : EditorWindow
{
  # region Variables
  private static int _toolbarIndex = 0;
  private static Dictionary<Type, Vector2> _scrollPositionByType = new();
  private static Vector2 _drawingEditorScrollPosition;
  private static Dictionary<Type, IdentifiedObject> _selectedObjectByType = new();
  
  private readonly Dictionary<Type, IODatabase> _databaseByType = new();
  private Type[] _databaseTypes;
  private string[] _databaseTypeNames;
  
  private Editor _cachedEditor;
  
  private Texture2D _selectedBoxTexture;
  private GUIStyle _selectedBoxStyle;
  #endregion

  #region UnityFunctions
  private void OnEnable()
  {
    SetupStyle();
    SetupDatabase(new[] { typeof(Category), typeof(Stat) });
  }
  private void OnDisable()
  {
    DestroyImmediate(_cachedEditor);
    DestroyImmediate(_selectedBoxTexture);
  }
  private void OnGUI()
  {
    _toolbarIndex = GUILayout.Toolbar(_toolbarIndex, _databaseTypeNames);

    EditorGUILayout.Space(4f);
    CustomEditorUtility.DrawUnderline();
    EditorGUILayout.Space(4f);

    DrawDatabase(_databaseTypes[_toolbarIndex]);
  }
  #endregion
  
  [MenuItem("Tools/Skill System")]
  private static void OpenWindow()
  {
    var window = GetWindow<SkillSystemWindow>("Skill System");
    window.minSize = new Vector2(800, 700);
    window.Show();
  }
  
  private void SetupStyle()
  {
    _selectedBoxTexture = new Texture2D(1, 1);
    _selectedBoxTexture.SetPixel(0, 0, new Color(0.31f, 0.40f, 0.50f));
    _selectedBoxTexture.Apply();
    _selectedBoxTexture.hideFlags = HideFlags.DontSave;
    
    _selectedBoxStyle = new GUIStyle();
    _selectedBoxStyle.normal.background = _selectedBoxTexture;
  }

  private void SetupDatabase(Type[] dataTypes)
  {
    if (_databaseByType.Count == 0)
    {
      if (!AssetDatabase.IsValidFolder("Assets/@Resources/Database"))
        AssetDatabase.CreateFolder("Assets/@Resources", "Database");

      foreach (var type in dataTypes)
      {
        var database = AssetDatabase.LoadAssetAtPath<IODatabase>($"Assets/@Resources/Database/{type.Name}Database.asset");
        if (database == null)
        {
          database = CreateInstance<IODatabase>();
          AssetDatabase.CreateAsset(database, $"Assets/@Resources/Database/{type.Name}Database.asset");
          AssetDatabase.CreateFolder("Assets/@Resources", type.Name);
        }

        _databaseByType[type] = database;
        _scrollPositionByType[type] = Vector2.zero;
        _selectedObjectByType[type] = null;
      }

      _databaseTypeNames = dataTypes.Select(x => x.Name).ToArray();
      _databaseTypes = dataTypes;
    }
  }

  private void DrawDatabase(Type dataType)
  {
    var database = _databaseByType[dataType];

    AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(32, 32 + database.GetCount));

    EditorGUILayout.BeginHorizontal();
    {
      EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f));
      {
        GUI.color = Color.green;
        if (GUILayout.Button($"New {dataType.Name}"))
        {
          var guid = Guid.NewGuid();
          var newData = CreateInstance(dataType) as IdentifiedObject;
          
          dataType.BaseType.GetField("_codeName", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(newData, guid.ToString());
          
          AssetDatabase.CreateAsset(newData, $"Assets/@Resources/{dataType.Name}/{dataType.Name.ToUpper()}_{guid}.asset");

          database.Add(newData);
          
          EditorUtility.SetDirty(database);
          AssetDatabase.SaveAssets();
          
          _selectedObjectByType[dataType] = newData;
        }
        
        GUI.color = Color.red;
        if (GUILayout.Button($"Remove Last {dataType.Name}"))
        {
          var lastData = database.GetCount > 0 ? database.GetData.Last() : null;
          if (lastData)
          {
            database.Remove(lastData);
            
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(lastData));
            
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
          }
        }
        
        GUI.color = Color.cyan;
        if (GUILayout.Button($"Sort By Name"))
        {
          database.SortByCodeName();
          
          EditorUtility.SetDirty(database);
          AssetDatabase.SaveAssets();
        }
        
        GUI.color = Color.white;
        EditorGUILayout.Space(2f);
        CustomEditorUtility.DrawUnderline();
        EditorGUILayout.Space(4f);
        
        _scrollPositionByType[dataType] = EditorGUILayout.BeginScrollView(_scrollPositionByType[dataType], false, true,
          GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
        {
          foreach (var datum in database.GetData)
          {
            float labelWidth = datum.GetIcon != null ? 200f : 245f;
            
            var style = _selectedObjectByType[dataType] == datum ? _selectedBoxStyle : GUIStyle.none;
    
            EditorGUILayout.BeginHorizontal(style, GUILayout.Height(40f));
            {
              if (datum.GetIcon)
              {
                var preview = AssetPreview.GetAssetPreview(datum.GetIcon);
                GUILayout.Label(preview, GUILayout.Height(40f), GUILayout.Width(40f));
              }
              
              EditorGUILayout.LabelField(datum.GetCodeName, GUILayout.Width(labelWidth), GUILayout.Height(40f));
              
              EditorGUILayout.BeginVertical();
              {
                EditorGUILayout.Space(10f);
                GUI.color = Color.red;
                if (GUILayout.Button("x", GUILayout.Width(20f)))
                {
                  database.Remove(datum);
                  AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(datum));
                  
                  EditorUtility.SetDirty(database);
                  AssetDatabase.SaveAssets();
                }
              }
              EditorGUILayout.EndVertical();
              
              GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            if (datum == null) break;
            
            var lastRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
            {
              _selectedObjectByType[dataType] = datum;
              _drawingEditorScrollPosition = Vector2.zero;
              
              Event.current.Use();
            }
          }
        }
        EditorGUILayout.EndScrollView();
      }
      EditorGUILayout.EndVertical();

      if (_selectedObjectByType[dataType])
      {
        _drawingEditorScrollPosition = EditorGUILayout.BeginScrollView(_drawingEditorScrollPosition);
        {
          EditorGUILayout.Space(2f);
          Editor.CreateCachedEditor(_selectedObjectByType[dataType], null, ref _cachedEditor);
          _cachedEditor.OnInspectorGUI();
        }
        EditorGUILayout.EndScrollView();
      }
    }
    EditorGUILayout.EndHorizontal();
  }
}
