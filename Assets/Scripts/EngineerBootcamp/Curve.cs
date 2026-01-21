using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a list of 3D points representing a closed or open curve/outline,
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class Curve : MonoBehaviour
{
    // List of local-space points defining the curve.
    public List<Vector3> points;

    /// <summary>
    /// Applies the curve to a connected MeshCreator (e.g., Extrude), triggering a mesh regeneration using the defined curve points.
    /// </summary>
    public void Apply()
    {
        MeshCreator creator = GetComponent<MeshCreator>();
        if (creator != null)
        {
            creator.RecalculateMesh();
        }
    }
}
