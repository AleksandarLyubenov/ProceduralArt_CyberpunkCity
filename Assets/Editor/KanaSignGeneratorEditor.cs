using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KanaSignGenerator))]
public class KanaSignGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Regenerate Kana"))
        {
            KanaSignGenerator generator = (KanaSignGenerator)target;
            generator.GenerateKanaSign();
        }
    }
}
