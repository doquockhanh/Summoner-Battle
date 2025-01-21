using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        UnitData unitData = (UnitData)target;
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(unitData);
            AssetDatabase.SaveAssets();
        }
    }
} 