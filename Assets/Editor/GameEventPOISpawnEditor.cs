using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameEventPOISpawnSettings))]
public class GameEventPOISpawnEditor : Editor
{
    SerializedProperty poiVariable;
    public float test;
    [SerializeField]
    public SpawnSettings spawnSettings;

    public override void OnInspectorGUI()
    {
        GameEventPOISpawnSettings gameEvent = (GameEventPOISpawnSettings)target;
        float test;

        if (GUILayout.Button("Raise"))
        {
            //gameEvent.Raise(poiVariable, spawnSettings);
        }
    }
}
