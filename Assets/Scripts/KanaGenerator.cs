using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates stylized kana signs with extruded strokes, intended for use as decorations on buildings.
/// </summary>
public class KanaSignGenerator : MonoBehaviour, IBuildingDecoration
{
    public enum KanaType { Random, NU, FU, A }

    [Header("Settings")]
    public KanaType kanaType = KanaType.Random;        
    public float signHeight = 0.1f;                    
    public float tileSize = 0.05f;                     
    public Material wallMaterial; // Material for the extruded sides
    public Material faceMaterial;                      

    /// <summary>
    /// Entry point from the decoration system. Assigns materials and starts sign generation.
    /// </summary>
    public void Generate(Vector3 position, Transform parent, Material[] possibleMaterials)
    {
        if (possibleMaterials.Length > 0)
            wallMaterial = possibleMaterials[Random.Range(0, possibleMaterials.Length)];

        faceMaterial = wallMaterial; // Optionally could be separated

        GenerateKanaSign();
    }

    /// <summary>
    /// Adjusts local orientation, scale, and offset of the sign so it's properly visible and sized.
    /// </summary>
    private void ApplyOrientationFix()
    {
        transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);
        transform.localPosition += transform.up * -1.6f;
        transform.localScale = transform.localScale * 2f;
    }

    /// <summary>
    /// Generates a kana sign with the selected or randomized glyph.
    /// </summary>
    [ContextMenu("Generate Kana Sign")]
    public void GenerateKanaSign()
    {
        ClearOldStrokes();

        KanaType selectedType = kanaType;
        if (kanaType == KanaType.Random)
            selectedType = (KanaType)Random.Range(1, 4); // Skip 0 == Random

        switch (selectedType)
        {
            case KanaType.NU: GenerateNU(); break;
            case KanaType.FU: GenerateFU(); break;
            case KanaType.A: GenerateA(); break;
        }

        ApplyOrientationFix();
    }

    /// <summary>
    /// Deletes any previously generated strokes under this object.
    /// </summary>
    void ClearOldStrokes()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    /// <summary>
    /// Generates a kana and aligns it with world position and rotation (e.g., billboard or signboard).
    /// </summary>
    public void GenerateAndFace(Vector3 worldPosition, Quaternion rotation)
    {
        GenerateKanaSign();
        transform.position = worldPosition;
        transform.rotation = rotation;
    }

    /// <summary>
    /// Generates the strokes that form the kana ぬ (nu).
    /// </summary>
    void GenerateNU()
    {
        GenerateStroke(new List<Vector2> {
            new Vector2(0.1f, 1.3f), new Vector2(1.0f, 1.3f),
            new Vector2(1.0f, 1.4f), new Vector2(0.1f, 1.4f)
        }, "Stroke1");

        GenerateStroke(new List<Vector2> {
            new Vector2(0.6f, 1.1f), new Vector2(0.75f, 1.0f),
            new Vector2(0.8f, 1.05f), new Vector2(0.65f, 1.15f)
        }, "Stroke2");

        GenerateStroke(new List<Vector2> {
            new Vector2(0.0f, 0.4f), new Vector2(0.8f, 1.0f),
            new Vector2(0.85f, 0.9f), new Vector2(0.1f, 0.3f)
        }, "Stroke3");
    }

    /// <summary>
    /// Generates the strokes that form the kana ふ (fu).
    /// </summary>
    void GenerateFU()
    {
        GenerateStroke(new List<Vector2> {
            new Vector2(0.4f, 0.0f), new Vector2(0.5f, 0.0f),
            new Vector2(0.5f, 1.4f), new Vector2(0.4f, 1.4f)
        }, "Stroke1");

        GenerateStroke(new List<Vector2> {
            new Vector2(0.1f, 1.3f), new Vector2(1.0f, 0.5f),
            new Vector2(1.05f, 0.6f), new Vector2(0.15f, 1.4f)
        }, "Stroke2");

        GenerateStroke(new List<Vector2> {
            new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.0f),
            new Vector2(0.75f, 0.1f), new Vector2(0.35f, 0.6f)
        }, "Stroke3");
    }

    /// <summary>
    /// Generates the strokes that form the kana あ (a).
    /// </summary>
    void GenerateA()
    {
        GenerateStroke(new List<Vector2> {
            new Vector2(0.4f, 1.4f), new Vector2(0.5f, 1.4f),
            new Vector2(0.0f, 0.0f), new Vector2(-0.1f, 0.0f)
        }, "Stroke1");

        GenerateStroke(new List<Vector2> {
            new Vector2(0.9f, 1.4f), new Vector2(1.0f, 1.4f),
            new Vector2(0.5f, 0.0f), new Vector2(0.4f, 0.0f)
        }, "Stroke2");

        GenerateStroke(new List<Vector2> {
            new Vector2(0.2f, 0.7f), new Vector2(0.8f, 0.7f),
            new Vector2(0.8f, 0.8f), new Vector2(0.2f, 0.8f)
        }, "Stroke3");
    }

    /// <summary>
    /// Turns a 2D path into a 3D extruded mesh stroke, applies materials, and names it.
    /// </summary>
    void GenerateStroke(List<Vector2> shape, string name)
    {
        GameObject stroke = new GameObject(name);
        stroke.transform.SetParent(this.transform);
        stroke.transform.localPosition = Vector3.zero;

        // Add curve shape from 2D to 3D
        var curve = stroke.AddComponent<Curve>();
        curve.points = new List<Vector3>();
        foreach (var p in shape)
            curve.points.Add(new Vector3(p.x, 0, p.y));  // Convert 2D to 3D curve

        // Extrude
        var extrude = stroke.AddComponent<Extrude>();
        extrude.height = signHeight;
        extrude.tileSize = tileSize;

        // Ensure the kana has required rendering components
        MeshRenderer renderer = stroke.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = stroke.AddComponent<MeshRenderer>();

        MeshFilter filter = stroke.GetComponent<MeshFilter>();
        if (filter == null)
            filter = stroke.AddComponent<MeshFilter>();

        renderer.materials = new Material[] { wallMaterial, faceMaterial };

        curve.Apply();
        extrude.RecalculateMesh();
    }
}
