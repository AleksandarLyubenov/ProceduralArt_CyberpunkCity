using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages decorative elements placed on building façades.
/// Randomly spawns from a list of configured decorations per floor.
/// </summary>
public class BuildingDecorationManager : MonoBehaviour
{
    public List<BuildingDecoration> decorations;

    /// <summary>
    /// Attempts to place a decoration at a given world position and orientation,
    /// constrained by allowed floor range and spawn chance.
    /// </summary>
    /// <param name="position">World position to place the decoration.</param>
    /// <param name="rotation">World rotation (typically faces outward from the building).</param>
    /// <param name="parent">The building transform to parent the decoration under.</param>
    /// <param name="floor">The floor number this decoration would be attached to.</param>
    public void TryPlaceDecoration(Vector3 position, Quaternion rotation, Transform parent, int floor)
    {
        foreach (var deco in decorations)
        {
            // Skip decorations not valid for this floor
            if (floor < deco.minAllowedFloor || floor > deco.maxAllowedFloor)
                continue;

            // Random chance to spawn this decoration
            if (Random.Range(0, 100) <= deco.spawnChance)
            {
                // Instantiate the decoration prefab at the given location and rotation
                GameObject instance = Instantiate(deco.prefab, position, rotation, parent);

                IBuildingDecoration decorationScript = instance.GetComponent<IBuildingDecoration>();
                if (decorationScript != null)
                {
                    decorationScript.Generate(position, parent, deco.possibleMaterials);
                }
                break;
            }
        }
    }
}
