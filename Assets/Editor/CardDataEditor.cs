using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Card))]
public class CardDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        Card card = (Card)target;
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(card);
            AssetDatabase.SaveAssets();
        }
    }
} 