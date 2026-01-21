using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDecoration", menuName = "ProceduralCity/BuildingDecoration", order = 1)]

/// <summary>
/// ScriptableObject to store data about a decoration.
/// </summary>
public class BuildingDecoration : ScriptableObject
{
    public GameObject prefab;
    [Range(0, 100)]
    public int spawnChance = 50;
    public Material[] possibleMaterials;

    [Header("Floor restrictions")]
    public int minAllowedFloor = 2;  // floors start at 0
    public int maxAllowedFloor = 100; // default very high
}
