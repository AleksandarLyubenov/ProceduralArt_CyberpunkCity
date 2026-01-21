using UnityEngine;
using UnityEditor;

/// <summary>
/// Allows interactive resizing of city zones directly in the Scene view.
/// </summary>
[CustomEditor(typeof(CityZone))]
public class CityZoneEditor : Editor
{
    /// <summary>
    /// Draws a scale handle in the Scene view to allow resizing the zone.
    /// </summary>
    void OnSceneGUI()
    {
        // Get reference to the selected CityZone
        CityZone zone = (CityZone)target;
        if (zone == null)
            return;

        Vector3 position = zone.transform.position;

        Vector3 size3D = new Vector3(zone.size.x, 0, zone.size.y);

        EditorGUI.BeginChangeCheck();

        // Draw and update the scale handle in the Scene view
        Vector3 newSize = Handles.ScaleHandle(
            size3D,
            position,
            Quaternion.identity,
            HandleUtility.GetHandleSize(position)
        );

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(zone, "Resize Zone");
            zone.size = new Vector2(newSize.x, newSize.z); // Update only X and Z, as it is 2D
            EditorUtility.SetDirty(zone);
        }
    }
}
