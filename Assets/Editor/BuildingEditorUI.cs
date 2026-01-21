using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for the BuildingController component.
/// </summary>
[CustomEditor(typeof(BuildingController))]
public class BuildingControllerEditor : Editor
{
    private BuildingController controller;

    private void OnEnable()
    {
        controller = (BuildingController)target;
    }

    /// <summary>
    /// Draws the custom inspector GUI including a button for regeneration.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Generate In Editor"))
        {
            controller.Regenerate();
        }
    }

    /// <summary>
    /// Called in the scene view to draw handles for curve point manipulation.
    /// </summary>
    void OnSceneGUI()
    {
        if (controller.curvePoints == null)
            return;

        bool dirty = false;
        Transform handleTransform = controller.transform;
        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i < controller.curvePoints.Count; i++)
        {
            // Convert local curve point to world position
            Vector3 worldPoint = handleTransform.TransformPoint(controller.curvePoints[i]);

            // Draw a line between this point and the previous one
            if (i > 0)
            {
                Handles.color = Color.cyan;
                Handles.DrawLine(previousPoint, worldPoint);
            }

            previousPoint = worldPoint;

            // Draw a position handle and allow moving it
            EditorGUI.BeginChangeCheck();
            Vector3 movedPoint = Handles.PositionHandle(worldPoint, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(controller, "Move Curve Point");
                controller.curvePoints[i] = handleTransform.InverseTransformPoint(movedPoint);
                dirty = true;
            }
        }

        // Close the loop visually
        if (controller.curvePoints.Count > 1)
        {
            Handles.color = Color.cyan;
            Handles.DrawLine(
                handleTransform.TransformPoint(controller.curvePoints[controller.curvePoints.Count - 1]),
                handleTransform.TransformPoint(controller.curvePoints[0]));
        }

        if (dirty)
        {
            EditorUtility.SetDirty(controller);
            controller.Regenerate();
        }
    }
}
