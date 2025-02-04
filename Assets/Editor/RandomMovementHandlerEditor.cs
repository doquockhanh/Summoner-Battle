using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RandomMovementHandler))]
public class RandomMovementHandlerEditor : Editor
{
    private SerializedProperty mapWidthProp;
    private SerializedProperty mapHeightProp;
    private SerializedProperty gridSizeProp;

    private void OnEnable()
    {
        mapWidthProp = serializedObject.FindProperty("mapWidth");
        mapHeightProp = serializedObject.FindProperty("mapHeight");
        gridSizeProp = serializedObject.FindProperty("gridSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
        
        if (GUI.changed)
        {
            SceneView.RepaintAll();
        }
    }

    private void OnSceneGUI()
    {
        Debug.Log("OnSceneGUI Called"); // Debug log để kiểm tra
        RandomMovementHandler handler = (RandomMovementHandler)target;
        
        // Vẽ khung bản đồ
        Handles.color = Color.white;
        Vector3 topLeft = new Vector3(-handler.MapWidth/2, handler.MapHeight/2, 0);
        Vector3 topRight = new Vector3(handler.MapWidth/2, handler.MapHeight/2, 0);
        Vector3 bottomLeft = new Vector3(-handler.MapWidth/2, -handler.MapHeight/2, 0);
        Vector3 bottomRight = new Vector3(handler.MapWidth/2, -handler.MapHeight/2, 0);
        
        Handles.DrawLine(topLeft, topRight);
        Handles.DrawLine(topRight, bottomRight);
        Handles.DrawLine(bottomRight, bottomLeft);
        Handles.DrawLine(bottomLeft, topLeft);
        
        // Vẽ đường phân chia giữa 2 phe
        Handles.color = Color.yellow;
        Vector3 midTop = new Vector3(0, handler.MapHeight/2, 0);
        Vector3 midBottom = new Vector3(0, -handler.MapHeight/2, 0);
        Handles.DrawLine(midTop, midBottom);
        
        // Vẽ grid
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
        for (float x = -handler.MapWidth/2; x <= handler.MapWidth/2; x += handler.GridSize)
        {
            Handles.DrawLine(
                new Vector3(x, -handler.MapHeight/2, 0),
                new Vector3(x, handler.MapHeight/2, 0)
            );
        }
        for (float y = -handler.MapHeight/2; y <= handler.MapHeight/2; y += handler.GridSize)
        {
            Handles.DrawLine(
                new Vector3(-handler.MapWidth/2, y, 0),
                new Vector3(handler.MapWidth/2, y, 0)
            );
        }
    }
} 