using UnityEngine;

/// <summary>
/// Draws a green wireframe cube in the Scene view representing the city bounds.
/// </summary>
[ExecuteAlways]
public class NeighborhoodCityGeneratorGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        NeighborhoodProceduralCityGenerator cityGen = GetComponent<NeighborhoodProceduralCityGenerator>();
        if (cityGen == null)
            return;

        Gizmos.color = Color.green;

        Vector3 center = transform.position;

        Vector3 size = new Vector3(cityGen.cityWidth, 0, cityGen.cityDepth);

        // Draw a wireframe cube to show the city bounds
        Gizmos.DrawWireCube(center, size);
    }
}
