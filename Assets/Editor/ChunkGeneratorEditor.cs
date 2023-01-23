using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathGenerator))]
public class PathGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PathGenerator pathGen = (PathGenerator)target;

        if (DrawDefaultInspector())
        {
            if (pathGen.autoupdate) pathGen.DrawChunkInEditor();
        }

        if (GUILayout.Button("Generate"))
        {
            pathGen.DrawChunkInEditor();
        }
    }
}
