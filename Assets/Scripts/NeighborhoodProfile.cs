using UnityEngine;

/// <summary>
/// Defines a configurable profile for neighborhood zones in the procedural city.
/// Used to control building properties, visual appearance, and editor visualization per zone.
/// </summary>
[CreateAssetMenu(fileName = "NeighborhoodProfile", menuName = "ProceduralCity/Neighborhood Profile")]
public class NeighborhoodProfile : ScriptableObject
{
    [Header("General Info")]
    public string profileName = "Default Neighborhood";

    [Header("Building Parameters")]
    public int minFloors = 5;
    public int maxFloors = 15;
    public float plotFillFactor = 0.8f; // How much of the plot the building is allowed to occupy (0–1 is equal to 0-100%)

    [Header("Plot Size (Per Zone)")]
    public float minPlotWidth = 10f;
    public float maxPlotWidth = 30f;
    public float minPlotDepth = 10f;
    public float maxPlotDepth = 30f;

    [Header("Aspect Ratio Control")]
    [Tooltip("Maximum allowed width/depth ratio for plots in this neighborhood")]
    public float maxAspectRatio = 2.0f; // Prevents extremely narrow/wide plots using a ratio of width to height

    [Header("Materials")]
    public Material[] wallMaterials;
    public Material roofMaterial;
    public GameObject[] roofPrefabs; // Optional prefabs for decorative rooftop elements

    [Header("Editor Color")]
    public Color zoneColor = Color.white; // Color shown in the Unity Scene view to easilly identify this zone
}
