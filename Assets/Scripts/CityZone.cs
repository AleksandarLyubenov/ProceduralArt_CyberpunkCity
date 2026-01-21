using UnityEngine;

/// <summary>
/// Defines a rectangular zone within the city that uses a specific NeighborhoodProfile.
/// </summary>
[ExecuteAlways]
public class CityZone : MonoBehaviour
{
    public Vector2 size = new Vector2(50f, 50f);
    public NeighborhoodProfile profile;

    /// <summary>
    /// Returns the rectangular bounds of this zone in world-space.
    /// </summary>
    public Rect GetRect()
    {
        Vector3 pos = transform.position;

        return new Rect(
            pos.x - size.x / 2f,
            pos.z - size.y / 2f,
            size.x,
            size.y
        );
    }
}
