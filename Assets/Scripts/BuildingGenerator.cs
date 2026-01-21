using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Generates a building, utilizing a curve and an extrude to make floors.
/// </summary>
[RequireComponent(typeof(Curve))]
[RequireComponent(typeof(Extrude))]
public class BuildingGenerator : MonoBehaviour
{
    // Materials and prefabs for building construction
    public Material[] floorMaterials;
    public Material roofMaterial;
    public GameObject[] roofPrefabs;
    public int levelToSpawnRoofs = 50; // Minimum number of floors before roof extras are added
    public float tileSize = 2f;

    [Header("Floating Billboards")]
    public GameObject[] floatingBillboardPrefabs;
    [Range(0, 1)] public float billboardSpawnChance = 0.5f;
    public float billboardVerticalOffsetMin = 2f;
    public float billboardVerticalOffsetMax = 8f;

    public BuildingDecorationManager decorationManager;

    [System.Serializable]
    public class BillboardPrefabData
    {
        public GameObject prefab;
        public float width = 4f;
        public float height = 8f;
    }

    [System.Serializable]
    public class BillboardData
    {
        public BillboardPrefabData[] prefabs;
        public Material[] materials;
        public Light[] lights;
    }

    [Header("Side Billboards")]
    public BillboardData sideBillboardData;
    public int billboardFloorsMin = 2;
    public int billboardFloorsMax = 3;
    public int billboardMinFloor = 3;
    public int billboardMaxFloorOffsetFromTop = 2;
    public float billboardDepthOffset = 0.1f;

    /// <summary>
    /// Main building generation logic using a basic rectangular shape.
    /// </summary>
    public void GenerateBuilding(float width, float depth, float totalHeight, float floorHeight)
    {
        if (decorationManager == null)
        {
            Debug.LogWarning("Decoration Manager is not assigned!");
        }

        int floorCount = Mathf.RoundToInt(totalHeight / floorHeight);
        float buildingY = 0f;

        for (int i = 0; i < floorCount; i++)
        {
            GameObject floor = new GameObject("Floor_" + i);
            floor.transform.parent = this.transform;
            floor.transform.localPosition = new Vector3(0, buildingY, 0);

            // Create a rectangular curve for this floor
            Curve curve = floor.AddComponent<Curve>();
            curve.points = new List<Vector3>
            {
                new Vector3(-width / 2f, 0, -depth / 2f),
                new Vector3(width / 2f, 0, -depth / 2f),
                new Vector3(width / 2f, 0, depth / 2f),
                new Vector3(-width / 2f, 0, depth / 2f)
            };

            // Extrude floor geometry
            Extrude extrude = floor.AddComponent<Extrude>();
            extrude.height = floorHeight;
            extrude.tileSize = tileSize;

            // Assign random wall material (shared) + roof material
            MeshRenderer mr = floor.GetComponent<MeshRenderer>();
            Material wallMaterial = floorMaterials[Random.Range(0, floorMaterials.Length)];
            mr.materials = new Material[] { wallMaterial, roofMaterial };

            curve.Apply();
            extrude.RecalculateMesh();

            buildingY += floorHeight;
        }

        // Add special roof features if building is tall enough
        if (roofPrefabs != null && roofPrefabs.Length > 0 && floorCount >= levelToSpawnRoofs)
        {
            GameObject prefab = roofPrefabs[Random.Range(0, roofPrefabs.Length)];
            GameObject roof = Instantiate(prefab, this.transform);
            roof.transform.localPosition = new Vector3(0, buildingY, 0);

            var pointy = roof.GetComponent<PointyRoofGenerator>();
            if (pointy != null)
                pointy.Generate(width, depth);

            var antenna = roof.GetComponent<AntennaClusterGenerator>();
            if (antenna != null)
                antenna.Generate(width, depth);
        }

        GenerateBillboards(buildingY, floorCount);
        GenerateDecorations(width, depth, floorCount, floorHeight);
        GenerateSideBillboards(width, depth, floorCount, floorHeight);
    }

    /// <summary>
    /// Attempts to place decorative props on the side of buildings at random heights.
    /// </summary>
    private void GenerateDecorations(float width, float depth, int floorCount, float floorHeight)
    {
        if (decorationManager == null) return;

        float halfWidth = width / 2f;
        float halfDepth = depth / 2f;

        int minDecorationFloor = 2;
        int maxDecorationFloor = floorCount - 3;

        for (int i = minDecorationFloor; i <= maxDecorationFloor; i++)
        {
            int side = Random.Range(0, 4); // Front, Back, Left, Right
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            switch (side)
            {
                case 0: pos = new Vector3(0, i * floorHeight + floorHeight / 2f, halfDepth + 0.01f); rot = Quaternion.Euler(0, 0, 0); break;
                case 1: pos = new Vector3(0, i * floorHeight + floorHeight / 2f, -halfDepth - 0.01f); rot = Quaternion.Euler(0, 180, 0); break;
                case 2: pos = new Vector3(-halfWidth - 0.01f, i * floorHeight + floorHeight / 2f, 0); rot = Quaternion.Euler(0, -90, 0); break;
                case 3: pos = new Vector3(halfWidth + 0.01f, i * floorHeight + floorHeight / 2f, 0); rot = Quaternion.Euler(0, 90, 0); break;
            }

            decorationManager.TryPlaceDecoration(transform.TransformPoint(pos), rot, this.transform, i);
        }
    }

    /// <summary>
    /// Random chance to spawn a floating billboard at the top of the building.
    /// </summary>
    private void GenerateBillboards(float buildingY, int floorCount)
    {
        if (floorCount >= levelToSpawnRoofs && floatingBillboardPrefabs.Length > 0 && Random.value < billboardSpawnChance)
        {
            GameObject prefab = floatingBillboardPrefabs[Random.Range(0, floatingBillboardPrefabs.Length)];
            GameObject billboard = Instantiate(prefab, this.transform);

            float yOffset = Random.Range(billboardVerticalOffsetMin, billboardVerticalOffsetMax);
            billboard.transform.localPosition = new Vector3(0, buildingY + yOffset, 0);
        }
    }

    /// <summary>
    /// Allows buildings with a non-rectangular/square footprint (defined via curvePoints).
    /// </summary>
    public void GenerateBuildingWithCustomShape(float width, float depth, int floorCount, float floorHeight, List<Vector3> curvePoints)
    {
        if (decorationManager == null)
        {
            Debug.LogWarning("Decoration Manager is not assigned!");
        }

        float buildingY = 0f;

        for (int i = 0; i < floorCount; i++)
        {
            GameObject floor = new GameObject("Floor_" + i);
            floor.transform.parent = this.transform;
            floor.transform.localPosition = new Vector3(0, buildingY, 0);

            Curve curve = floor.AddComponent<Curve>();
            curve.points = new List<Vector3>(curvePoints);

            Extrude extrude = floor.AddComponent<Extrude>();
            extrude.height = floorHeight;
            extrude.tileSize = tileSize;

            MeshRenderer mr = floor.GetComponent<MeshRenderer>();
            Material wallMaterial = floorMaterials[Random.Range(0, floorMaterials.Length)];
            mr.materials = new Material[] { wallMaterial, roofMaterial };

            curve.Apply();
            extrude.RecalculateMesh();

            buildingY += floorHeight;
        }

        // Optional roof extras
        if (roofPrefabs != null && roofPrefabs.Length > 0 && floorCount >= 60)
        {
            GameObject prefab = roofPrefabs[Random.Range(0, roofPrefabs.Length)];
            GameObject roof = Instantiate(prefab, this.transform);
            roof.transform.localPosition = new Vector3(0, buildingY, 0);

            var pointy = roof.GetComponent<PointyRoofGenerator>();
            if (pointy != null)
                pointy.Generate(width, depth);

            var antenna = roof.GetComponent<AntennaClusterGenerator>();
            if (antenna != null)
                antenna.Generate(width, depth);
        }

        GenerateBillboards(buildingY, floorCount);
        GenerateDecorations(width, depth, floorCount, floorHeight);
        GenerateSideBillboards(width, depth, floorCount, floorHeight);
    }

    /// <summary>
    /// Attempts to place a few side-attached billboards on the building.
    /// Each billboard randomly chooses a side and prefab.
    /// </summary>
    private void GenerateSideBillboards(float width, float depth, int floorCount, float floorHeight)
    {
        if (sideBillboardData == null || sideBillboardData.prefabs.Length == 0 ||
            sideBillboardData.materials.Length == 0 || sideBillboardData.lights.Length == 0)
            return;

        int numBillboards = Random.Range(billboardFloorsMin, billboardFloorsMax + 1);

        for (int i = 0; i < numBillboards; i++)
        {
            int floorIndex = Random.Range(billboardMinFloor, floorCount - billboardMaxFloorOffsetFromTop);
            int side = Random.Range(0, 4);

            BillboardPrefabData selectedPrefabData = sideBillboardData.prefabs[Random.Range(0, sideBillboardData.prefabs.Length)];
            float prefabWidth = selectedPrefabData.width;
            float prefabHeight = selectedPrefabData.height;

            float remainingHeight = (floorCount * floorHeight) - (floorIndex * floorHeight);
            if (remainingHeight < prefabHeight) continue;

            // Ensure the prefab fits on the selected face
            bool canFit = (side < 2) ? width >= prefabWidth : depth >= prefabWidth;
            if (!canFit) continue;

            float horizontalOffsetMax = (side < 2 ? (width - prefabWidth) : (depth - prefabWidth)) / 2f;
            float horizontalOffset = Random.Range(-horizontalOffsetMax, horizontalOffsetMax);
            float yPos = floorIndex * floorHeight + floorHeight / 2f;

            Vector3 localPos = Vector3.zero;
            Quaternion localRot = Quaternion.identity;

            switch (side)
            {
                case 0: localPos = new Vector3(horizontalOffset, yPos, depth / 2f + billboardDepthOffset); localRot = Quaternion.Euler(0, 0, 0); break;
                case 1: localPos = new Vector3(horizontalOffset, yPos, -depth / 2f - billboardDepthOffset); localRot = Quaternion.Euler(0, 180, 0); break;
                case 2: localPos = new Vector3(-width / 2f - billboardDepthOffset, yPos, horizontalOffset); localRot = Quaternion.Euler(0, -90, 0); break;
                case 3: localPos = new Vector3(width / 2f + billboardDepthOffset, yPos, horizontalOffset); localRot = Quaternion.Euler(0, 90, 0); break;
            }

            GameObject billboard = Instantiate(selectedPrefabData.prefab, this.transform);
            billboard.name = $"SideBillboard_Floor_{floorIndex}_Side_{side}";
            billboard.transform.localPosition = localPos;
            billboard.transform.localRotation = localRot;

            int matLightIndex = Random.Range(0, Mathf.Min(sideBillboardData.materials.Length, sideBillboardData.lights.Length));

            Renderer rend = billboard.GetComponentInChildren<Renderer>();
            if (rend != null)
                rend.material = sideBillboardData.materials[matLightIndex];

            Light light = Instantiate(sideBillboardData.lights[matLightIndex], billboard.transform);
            light.transform.localPosition = Vector3.zero;
        }
    }
}
