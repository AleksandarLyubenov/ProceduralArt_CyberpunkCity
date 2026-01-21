using UnityEngine;
using UnityEditor;

/// <summary>
/// Adds buttons to generate or clear the city directly from the Inspector.
/// </summary>
[CustomEditor(typeof(NeighborhoodProceduralCityGenerator))]
public class NeighborhoodCityGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        NeighborhoodProceduralCityGenerator cityGen = (NeighborhoodProceduralCityGenerator)target;

        // Add spacing between the default inspector and our buttons
        GUILayout.Space(10);

        // Button to trigger city generation at runtime or in editor mode
        if (GUILayout.Button("Generate City"))
        {
            cityGen.GenerateCity();
        }

        // Button to clear the currently generated city
        if (GUILayout.Button("Despawn City"))
        {
            cityGen.ClearCity();
        }
    }
}
