using UnityEngine;

/// <summary>
/// Generates a pointy roof on the top of large buildings.
/// </summary>
public class PointyRoofGenerator : MonoBehaviour
{
    [Header("Randomization Settings")]
    public int minSegments = 5;
    public int maxSegments = 10;
    public float minShrinkFactor = 0.5f;
    public float maxShrinkFactor = 0.8f;
    public float heightMultiplier = 1.0f;
    public Material roofMaterial;

    [Header("UV Settings")]
    public float tileSize = 2f;

    public void Generate(float baseWidth, float baseDepth)
    {
        int segments = Random.Range(minSegments, maxSegments + 1);
        float widthShrinkFactor = Random.Range(minShrinkFactor, maxShrinkFactor);

        float segmentHeight = Mathf.Min(baseWidth, baseDepth) * 0.2f * heightMultiplier;

        float currentWidth = baseWidth;
        float currentDepth = baseDepth;
        float currentY = 0f;

        for (int i = 0; i < segments; i++)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.transform.SetParent(transform);
            part.transform.localPosition = new Vector3(0, currentY + segmentHeight / 2f, 0);
            part.transform.localScale = new Vector3(currentWidth, segmentHeight, currentDepth);
            part.GetComponent<MeshRenderer>().material = roofMaterial;

            // Apply AutoUV immediately after creating the cube
            AutoUv autoUV = part.AddComponent<AutoUv>();
            autoUV.textureScaleFactor = new Vector2(tileSize, tileSize);
            autoUV.UpdateUvs();

            currentY += segmentHeight;
            currentWidth *= widthShrinkFactor;
            currentDepth *= widthShrinkFactor;

            if (currentWidth < 0.1f || currentDepth < 0.1f)
                break;
        }
    }
}
