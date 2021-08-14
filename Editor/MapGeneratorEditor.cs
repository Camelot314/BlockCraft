using UnityEditor;
using UnityEngine;

/// <summary>
/// Adds a button to the map generator script as well as updates the map
/// whenever a value in the inspector is changed and the autoUpdate is true. 
/// </summary>
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{


    public override void OnInspectorGUI()
    {

        MapGenerator mapGen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.IsAutoUpdate())
            {
                MeshGenerator.SetChunkHeight(32);
                mapGen.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            MeshGenerator.SetChunkHeight(32);
            mapGen.DrawMapInEditor();
        }


    }
}
