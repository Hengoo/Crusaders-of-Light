using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelCreator))]
public class MapPreviewEditor : Editor {
    public override void OnInspectorGUI()
    {
        LevelCreator levelCreator = (LevelCreator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate"))
        {
            levelCreator.GeneratePreview();
        }
    }
}
