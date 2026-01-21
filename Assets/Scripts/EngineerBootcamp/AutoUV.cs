// Version 2023
//  (Updates: no getters in loops)
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatically generates UVs for a mesh based on world or local space projections.
/// </summary>
public class AutoUv : MonoBehaviour
{
	public Vector2 textureScaleFactor = new Vector2(1, 1);
	public bool UseWorldCoordinates;
	public bool AutoUpdate;
	public bool RecalculateTangents = true;

    private Dictionary<int, Vector3> tangentCache = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> bitangentCache = new Dictionary<int, Vector3>();

    void Update()
	{
		if ((transform.hasChanged && UseWorldCoordinates && AutoUpdate) || Input.GetKeyDown(KeyCode.F2))
		{
			UpdateUvs();
			transform.hasChanged = false;
		}
	}

    /// <summary>
    /// Updates UVs for a given mesh based on the projection onto a generated tangent plane.
    /// </summary>
    public void UpdateUVs(Mesh mesh)
    {
        Debug.Log("Updating UVs");

        Vector2[] uv = mesh.uv;
        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        tangentCache.Clear();
        bitangentCache.Clear();

        for (int i = 0; i < tris.Length; i += 3)
        {
            int i1 = tris[i];
            int i2 = tris[i + 1];
            int i3 = tris[i + 2];
            Vector3 v1 = verts[i1];
            Vector3 v2 = verts[i2];
            Vector3 v3 = verts[i3];
            if (UseWorldCoordinates)
            {
                v1 = transform.TransformPoint(v1);
                v2 = transform.TransformPoint(v2);
                v3 = transform.TransformPoint(v3);
            }

            Vector3 tangent, biTangent;

            // Store the computed tangent &bitangent for each vertex.
			// If those have already been computed for at least one of the triangle vertices, assign those to all triangle vertices, instead of recomputing.
            if (tangentCache.ContainsKey(i1))
            {
                tangent = tangentCache[i1];
                biTangent = bitangentCache[i1];
            }
            else
            {
                ComputeTangents(v1, v2, v3, out tangent, out biTangent);
                tangentCache[i1] = tangent;
                bitangentCache[i1] = biTangent;
            }

            ComputeTriangleUVs(v1, v2, v3, ref uv[i1], ref uv[i2], ref uv[i3], tangent, biTangent);
        }
        mesh.uv = uv;
        if (RecalculateTangents)
        {
            mesh.RecalculateTangents();
        }
    }

    /// <summary>
    /// Clones the mesh and updates UVs on the instance (non-shared) to prevent leaks.
    /// </summary>
    public void UpdateUvs()
    {
        // Clone the shared mesh manually, to prevent the "leaking meshes" error:
        Mesh origMesh = GetComponent<MeshFilter>().sharedMesh;
        Mesh mesh = (Mesh)Instantiate(origMesh);

        UpdateUVs(mesh);

        GetComponent<MeshFilter>().mesh = mesh;
    }

    /// <summary>
    /// Computes orthonormal tangent and bitangent vectors for the triangle.
    /// </summary>
    void ComputeTangents(Vector3 v1, Vector3 v2, Vector3 v3, out Vector3 tangent, out Vector3 biTangent)
    {
        Vector3 edge1 = v2 - v1;
        Vector3 edge2 = v3 - v1;
        Vector3 normal = Vector3.Cross(edge1, edge2);

        // If the triangle has almost zero area, the normal will be small as well, but then the uvs won't matter much anyway:
        if (normal.magnitude <= 0.000001)
        {
            tangent = new Vector3();
            biTangent = new Vector3();
            return;
        }
        normal.Normalize();

        // Compute a tangent that is perpendicular to both the up vector and the normal using a cross product:
        tangent = Vector3.Cross(Vector3.up, normal).normalized;
        if (tangent == Vector3.zero)
            tangent = Vector3.right;

        // Compute a bitangent that is perpendicular to both the normal and the tangent:
        biTangent = Vector3.Cross(normal, tangent).normalized;
    }

    /// <summary>
    /// Projects triangle vertices onto tangent space and assigns UVs using scalar projection.
    /// </summary>
    void ComputeTriangleUVs(Vector3 v1, Vector3 v2, Vector3 v3, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3, Vector3 tangent, Vector3 biTangent)
    {
        // Scalar Projection
        uv1 = new Vector2(Vector3.Dot(v1, tangent), Vector3.Dot(v1, biTangent)) / textureScaleFactor;
        uv2 = new Vector2(Vector3.Dot(v2, tangent), Vector3.Dot(v2, biTangent)) / textureScaleFactor;
        uv3 = new Vector2(Vector3.Dot(v3, tangent), Vector3.Dot(v3, biTangent)) / textureScaleFactor;
    }
}
