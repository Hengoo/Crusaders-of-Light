﻿using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor {
    public override void OnInspectorGUI()
    {
        MapPreviewOld mapPreview = (MapPreviewOld)target;

        if(DrawDefaultInspector())
            if(mapPreview.autoUpdate)
                mapPreview.DrawMapInEditor();

        if (GUILayout.Button("Generate"))
        {
            mapPreview.DrawMapInEditor();
        }
    }
}
