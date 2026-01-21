using UnityEngine;

/// <summary>
/// Generates antennas on the top of large buildings.
/// </summary>
public class AntennaClusterGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int maxAntennas = 5;
    public float antennaHeight = 10f;
    public float antennaRadius = 0.2f;
    public Material antennaMaterial;

    [Header("UV Settings")]
    public float tileSize = 2f;

    [Header("Anti-Collision Light")]
    public Material antiCollisionMaterial;
    public float antiCollisionSphereRadius = 3f;

    public void Generate(float baseWidth, float baseDepth)
    {
        int antennaCount = Random.Range(1, maxAntennas + 1);

        for (int i = 0; i < antennaCount; i++)
        {
            // Create antenna cylinder
            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.transform.SetParent(transform);

            float x = Random.Range(-baseWidth / 2f + antennaRadius, baseWidth / 2f - antennaRadius);
            float z = Random.Range(-baseDepth / 2f + antennaRadius, baseDepth / 2f - antennaRadius);

            antenna.transform.localPosition = new Vector3(x, antennaHeight / 2f, z);
            antenna.transform.localScale = new Vector3(antennaRadius, antennaHeight / 2f, antennaRadius);
            antenna.GetComponent<MeshRenderer>().material = antennaMaterial;

            // Apply AutoUV
            AutoUv autoUV = antenna.AddComponent<AutoUv>();
            autoUV.UseWorldCoordinates = true;
            autoUV.textureScaleFactor = new Vector2(tileSize, tileSize);
            autoUV.UpdateUvs();

            // Create anti-collision light sphere
            GameObject lightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lightSphere.transform.SetParent(transform);
            lightSphere.transform.localPosition = new Vector3(x, antennaHeight, z);
            lightSphere.transform.localScale = Vector3.one * antiCollisionSphereRadius;
            lightSphere.GetComponent<MeshRenderer>().material = antiCollisionMaterial;

            // Remove sphere collider
            SafeDestroy.DestroyObject(lightSphere.GetComponent<Collider>());
        }
    }
}
