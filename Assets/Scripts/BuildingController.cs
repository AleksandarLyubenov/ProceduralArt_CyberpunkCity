using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls procedural generation and regeneration of a building
/// using parameters or a custom base curve.
/// </summary>
[ExecuteAlways]
public class BuildingController : MonoBehaviour
{
    [Header("Building Parameters")]
    public float width;                     
    public float depth;                     
    public int floorCount;                  
    public float floorHeight;               
    public List<Vector3> curvePoints = new();  // Optional custom footprint shape

    private BuildingGenerator generator;

    /// <summary>
    /// Called during play mode and editor initialization.
    /// </summary>
    void Awake()
    {
        generator = GetComponent<BuildingGenerator>();
    }

    /// <summary>
    /// Ensures generator is loaded when entering editor mode.
    /// Called on script enable in the Editor.
    /// </summary>
    private void OnEnable()
    {
        if (generator == null)
            generator = GetComponent<BuildingGenerator>();
    }

    /// <summary>
    /// Initializes the controller with procedural building data.
    /// </summary>
    public void Initialize(float width, float depth, int floorCount, float floorHeight, List<Vector3> curvePoints)
    {
        this.width = width;
        this.depth = depth;
        this.floorCount = floorCount;
        this.floorHeight = floorHeight;
        this.curvePoints = new List<Vector3>(curvePoints); // Copy to avoid shared list reference
    }

    /// <summary>
    /// Clears previous generated floors and regenerates building from parameters.
    /// </summary>
    public void Regenerate()
    {
        // Remove all previously generated children (floors, roofs, etc.)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Generate new building using stored parameters
        generator.GenerateBuildingWithCustomShape(width, depth, floorCount, floorHeight, curvePoints);
    }
}
