using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapPreviewEditor : Editor {
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            mapGenerator.GeneratePreview();
        }
    }
}
