using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Generates a 3D mesh by extruding a 2D polygon vertically.
/// Requires a Curve component for input points, and a MeshFilter & MeshRenderer for output.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Extrude : MeshCreator
{
    public float height = 10f; // Height of the extrusion (building floor)
    public bool ModifySharedMesh = false; // Whether to modify the shared mesh (dangerous if shared)
    public float tileSize = 2f; // UV tiling size

    /// <summary>
    /// Recalculates the mesh by extruding a curve vertically into 3D geometry.
    /// </summary>
    public override void RecalculateMesh()
    {
        Curve curve = GetComponent<Curve>();
        if (curve == null)
            return;

        // Clean and validate the input points
        List<Vector3> points = CleanPoints(curve.points);
        if (points.Count < 3)
        {
            Debug.LogError("Cannot triangulate polygons with less than 3 vertices");
            return;
        }

        // Convert to 2D polygon (XZ-plane)
        List<Vector2> polygon = new List<Vector2>();
        for (int i = 0; i < points.Count; i++)
            polygon.Add(new Vector2(points[i].x, points[i].z));

        // Ensure the polygon is clockwise
        if (!IsClockwise(polygon))
        {
            polygon.Reverse();
            points.Reverse();
        }

        // Create a list of indices 0..n-1:
        List<int> indices = new List<int>();
        for (int i = 0; i < polygon.Count; i++)
            indices.Add(i);

        // This list is going to contain the vertex indices of the triangles: (3 integers per triangle)
        List<int> triangles = new List<int>();
        
        // Compute the triangulation of [polygon], store it in [triangles]:
        try
        {
            TriangulatePolygon(triangles, polygon, indices);
        }
        catch (Exception e)
        {
            Debug.LogError("Triangulation failed: " + e.Message);
            return;
        }

        // Begin building the mesh
        MeshBuilder builder = new MeshBuilder();

        // BOTTOM FACE (submesh 1)
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pos = new Vector3(points[i].x, 0, points[i].z);
            Vector2 uv = new Vector2(pos.x / tileSize, pos.z / tileSize);
            builder.AddVertex(pos, uv);
        }
        for (int t = 0; t < triangles.Count; t += 3)
            builder.AddTriangle(triangles[t], triangles[t + 1], triangles[t + 2], 1);

        // TOP FACE (submesh 1)
        int n = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pos = new Vector3(points[i].x, height, points[i].z);
            Vector2 uv = new Vector2(pos.x / tileSize, pos.z / tileSize);
            builder.AddVertex(pos, uv);
        }
        for (int t = 0; t < triangles.Count; t += 3)
            builder.AddTriangle(n + triangles[t], n + triangles[t + 1], n + triangles[t + 2], 1);

        // SIDE WALLS (submesh 0)
        for (int i = 0; i < points.Count; i++)
        {
            int j = (i + 1) % points.Count;

            Vector3 p1 = new Vector3(points[i].x, 0, points[i].z);
            Vector3 p2 = new Vector3(points[j].x, 0, points[j].z);
            float segmentLength = Vector3.Distance(p1, p2);

            // Add UVs
            float uMax = segmentLength / tileSize;
            float vMax = height / tileSize;

            // Add Vertices
            int v1 = builder.AddVertex(p1, new Vector2(0, 0));
            int v2 = builder.AddVertex(p2, new Vector2(uMax, 0));
            int v3 = builder.AddVertex(p1 + Vector3.up * height, new Vector2(0, vMax));
            int v4 = builder.AddVertex(p2 + Vector3.up * height, new Vector2(uMax, vMax));

            // Add quad
            builder.AddTriangle(v1, v2, v3, 0);
            builder.AddTriangle(v2, v4, v3, 0);
        }

        // Finalize mesh
        ReplaceMesh(builder.CreateMesh(), ModifySharedMesh);
    }

    /// <summary>
    /// Ear clipping triangulation for convex/concave polygons.
    /// </summary>
    void TriangulatePolygon(List<int> triangles, List<Vector2> polygon, List<int> indices)
    {
        while (polygon.Count >= 3)
        {
            bool earFound = false;
            for (int i = 0; i < polygon.Count; i++)
            {
                int i0 = (i + polygon.Count - 1) % polygon.Count;
                int i1 = i;
                int i2 = (i + 1) % polygon.Count;

                Vector2 a = polygon[i0];
                Vector2 b = polygon[i1];
                Vector2 c = polygon[i2];

                // Clockwise check
                if (!Clockwise(a, b, c)) continue;

                bool hasPointInside = false;
                for (int j = 0; j < polygon.Count; j++)
                {
                    if (j == i0 || j == i1 || j == i2) continue;
                    if (InsideTriangle(a, b, c, polygon[j]))
                    {
                        hasPointInside = true;
                        break;
                    }
                }

                if (hasPointInside) continue;

                // Add a triangle on u,v,w
                triangles.Add(indices[i0]);
                triangles.Add(indices[i1]);
                triangles.Add(indices[i2]);

                polygon.RemoveAt(i1); // remove v from point list (keep u and w)
                indices.RemoveAt(i1);
                earFound = true;
                break;
            }

            if (!earFound)
                throw new Exception("No suitable triangulation found.");
        }
    }

    /// <summary>
    /// Check if polygon is clockwise.
    /// </summary>
    bool IsClockwise(List<Vector2> poly)
    {
        float sum = 0f;
        for (int i = 0; i < poly.Count; i++)
        {
            Vector2 v1 = poly[i];
            Vector2 v2 = poly[(i + 1) % poly.Count];
            sum += (v2.x - v1.x) * (v2.y + v1.y);
        }
        return sum > 0f;
    }

    /// <summary>
    /// Checks if three points define a clockwise triangle.
    /// </summary>
    bool Clockwise(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        Vector2 d1 = (p2 - p1);
        Vector2 d2 = (p3 - p2);
        return (-d1.y * d2.x + d1.x * d2.y) < 0;
    }

    /// <summary>
    /// Check if a point lies inside the triangle defined by (a, b, c).
    /// </summary>
    bool InsideTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        bool b1 = Sign(p, a, b) < 0.0f;
        bool b2 = Sign(p, b, c) < 0.0f;
        bool b3 = Sign(p, c, a) < 0.0f;
        return ((b1 == b2) && (b2 == b3));
    }

    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    /// <summary>
    /// Cleans redundant points and closes the loop if necessary.
    /// </summary>
    List<Vector3> CleanPoints(List<Vector3> input)
    {
        List<Vector3> result = new List<Vector3>();
        foreach (var p in input)
        {
            if (result.Count == 0 || Vector3.Distance(p, result[result.Count - 1]) > 0.0001f)
                result.Add(p);
        }

        // Remove closing point if it's the same as the first one
        if (result.Count >= 2 && Vector3.Distance(result[0], result[result.Count - 1]) < 0.0001f)
            result.RemoveAt(result.Count - 1);

        return result;
    }
}
