using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Curve))]
public class CurveEditor : Editor
{
    private Curve curve;

    private void OnEnable()
    {
        curve = (Curve)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Apply Curve"))
        {
            Undo.RecordObject(curve, "Apply Curve");
            curve.Apply();
            EditorUtility.SetDirty(curve);
        }

        DrawDefaultInspector();
    }

    void OnSceneGUI()
    {
        if (curve.points == null)
            return;

        bool dirty = false;

        dirty |= ShowAndMovePoints();

        if (dirty)
        {
            // Apply mesh changes after moving points
            curve.Apply();

            // Mark as dirty for undo system
            Undo.RecordObject(curve, "Move Curve Point");
            EditorUtility.SetDirty(curve);
        }
    }

    bool ShowAndMovePoints()
    {
        bool dirty = false;
        Transform handleTransform = curve.transform;

        Vector3 previousPoint = Vector3.zero;
        for (int i = 0; i < curve.points.Count; i++)
        {
            Vector3 worldPoint = handleTransform.TransformPoint(curve.points[i]);

            if (i > 0)
            {
                Handles.color = Color.white;
                Handles.DrawLine(previousPoint, worldPoint);
            }
            previousPoint = worldPoint;

            EditorGUI.BeginChangeCheck();
            Vector3 movedPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                curve.points[i] = handleTransform.InverseTransformPoint(movedPoint);
                dirty = true;
            }
        }
        return dirty;
    }
}