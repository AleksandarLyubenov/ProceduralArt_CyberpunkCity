using UnityEngine;

[ExecuteAlways]
public class CityZoneGizmoDrawer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        CityZone zone = GetComponent<CityZone>();
        if (zone == null || zone.profile == null)
            return;

        Rect rect = zone.GetRect();
        Vector3 center = new Vector3(rect.x + rect.width / 2f, 0, rect.y + rect.height / 2f);
        Vector3 size3D = new Vector3(rect.width, 0, rect.height);

        // Draw filled color
        Color fillColor = new Color(zone.profile.zoneColor.r, zone.profile.zoneColor.g, zone.profile.zoneColor.b, 0.15f);
        Gizmos.color = fillColor;
        Gizmos.DrawCube(center, size3D);

        // Draw outline
        Gizmos.color = zone.profile.zoneColor;
        Gizmos.DrawWireCube(center, size3D);
    }
}
