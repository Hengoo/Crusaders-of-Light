using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor {
    public override void OnInspectorGUI()
    {
        MapPreview mapPreview = (MapPreview)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            mapPreview.GeneratePreview();
        }
    }
}
