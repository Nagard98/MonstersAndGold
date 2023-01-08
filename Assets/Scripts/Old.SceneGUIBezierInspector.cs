/*using UnityEngine;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(SceneGUIBezier))]
public class SceneGUIBezierInspector : Editor
{
    void OnSceneGUI()
    {
        var script = (SceneGUIBezier)target;

        script.PointA = Handles.PositionHandle(script.PointA, Quaternion.identity);
        script.PointB = Handles.PositionHandle(script.PointB, Quaternion.identity);
        //script.TangentA = Handles.PositionHandle(script.TangentA, Quaternion.identity);
        script.TangentB = Handles.PositionHandle(script.TangentB, Quaternion.identity);
        script.TangentA = script.PointA;

        Handles.DrawBezier(script.PointA, script.PointB, script.TangentA, script.TangentB, Color.red, null, 5);
    }

    const string resourceFilename = "test";
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement customInspector = new VisualElement();
        var visualTree = Resources.Load(resourceFilename) as VisualTreeAsset;
        visualTree.CloneTree(customInspector);
        customInspector.styleSheets.Add(Resources.Load($"{resourceFilename}-style") as StyleSheet);
        return customInspector;
    }
}*/