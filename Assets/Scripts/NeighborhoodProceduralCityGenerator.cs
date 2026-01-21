using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PlotDecorationData
{
    public GameObject prefab;
    public Vector3 localRotationOffset;
}

/// <summary>
/// Generates procedural city blocks with roads, plots, buildings, decorations, and fog.
/// </summary>
public class NeighborhoodProceduralCityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public float cityWidth = 100f;
    public float cityDepth = 100f;

    [Header("Global Plot Subdivision Settings (used for splitting only)")]
    public float subdivisionMinPlotWidth = 10f;
    public float subdivisionMinPlotDepth = 10f;
    public float subdivisionMaxPlotWidth = 30f;
    public float subdivisionMaxPlotDepth = 30f;
    public float subdivisionMaxAspectRatio = 2f;
    public float roadWidth = 4f;

    [Header("Ground Fog")]
    public GameObject fogPlanePrefab;

    [Header("Buildings")]
    public GameObject buildingPrefab;
    public float buildingGridSize = 1f;
    public float floorHeight = 2f;

    [Header("Plot Decorations")]
    public PlotDecorationData[] plotDecorations;
    [Range(0f, 1f)]
    public float decorationSpawnChance = 0.3f;

    [Header("Building Decorations")]
    public BuildingDecorationManager buildingDecorationManager;

    [Header("Zoning")]
    public List<CityZone> cityZones;
    public NeighborhoodProfile defaultProfile;

    [Header("Materials")]
    public Material plotMaterial;
    public Material roadMaterial;
    public Material roadLineMaterial;

    // Internal lists of generated plots and roads
    private List<Rect> plots = new List<Rect>();
    private List<Rect> roadSegments = new List<Rect>();

    void Start()
    {
        if (Application.isPlaying)
        {
            GenerateCity();
        }
    }

    /// <summary>
    /// Main entry point to generate a full city.
    /// </summary>
    [ContextMenu("Generate City")]
    public void GenerateCity()
    {
        ClearCity();

        // Start with the entire city area and recursively divide it
        Rect initialRect = new Rect(-cityWidth / 2f, -cityDepth / 2f, cityWidth, cityDepth);
        Subdivide(initialRect);

        SpawnCityFog(initialRect);
        GeneratePlots();
        GenerateRoads();
    }

    /// <summary>
    /// Recursively subdivides a large area into smaller plots and roads (Binary Space Partitioning).
    /// </summary>
    void Subdivide(Rect area)
    {
        float aspectRatio = Mathf.Max(area.width / area.height, area.height / area.width);
        bool widthOk = area.width <= subdivisionMaxPlotWidth;
        bool depthOk = area.height <= subdivisionMaxPlotDepth;
        bool ratioOk = aspectRatio <= subdivisionMaxAspectRatio;

        // If plot is acceptable, add it
        if (widthOk && depthOk && ratioOk)
        {
            plots.Add(area);
            return;
        }

        // Decide splitting direction based on overshoot
        bool splitHorizontally;
        float widthOvershoot = area.width - subdivisionMaxPlotWidth;
        float heightOvershoot = area.height - subdivisionMaxPlotDepth;

        if (!widthOk && !depthOk)
            splitHorizontally = heightOvershoot >= widthOvershoot;
        else if (!widthOk)
            splitHorizontally = false;
        else if (!depthOk)
            splitHorizontally = true;
        else
            splitHorizontally = area.height > area.width;

        // Actually perform split
        if (splitHorizontally)
        {
            float minSplit = subdivisionMinPlotDepth + roadWidth / 2f;
            float maxSplit = area.height - subdivisionMinPlotDepth - roadWidth / 2f;

            if (maxSplit > minSplit)
            {
                float splitY = Random.Range(minSplit, maxSplit);
                Rect bottom = new Rect(area.x, area.y, area.width, splitY - roadWidth / 2f);
                Rect top = new Rect(area.x, area.y + splitY + roadWidth / 2f, area.width, area.height - splitY - roadWidth / 2f);
                Rect road = new Rect(area.x, area.y + splitY - roadWidth / 2f, area.width, roadWidth);

                roadSegments.Add(road);
                Subdivide(bottom);
                Subdivide(top);
            }
            else
            {
                plots.Add(area);
            }
        }
        else
        {
            float minSplit = subdivisionMinPlotWidth + roadWidth / 2f;
            float maxSplit = area.width - subdivisionMinPlotWidth - roadWidth / 2f;

            if (maxSplit > minSplit)
            {
                float splitX = Random.Range(minSplit, maxSplit);
                Rect left = new Rect(area.x, area.y, splitX - roadWidth / 2f, area.height);
                Rect right = new Rect(area.x + splitX + roadWidth / 2f, area.y, area.width - splitX - roadWidth / 2f, area.height);
                Rect road = new Rect(area.x + splitX - roadWidth / 2f, area.y, roadWidth, area.height);

                roadSegments.Add(road);
                Subdivide(left);
                Subdivide(right);
            }
            else
            {
                plots.Add(area);
            }
        }
    }

    /// <summary>
    /// Spawns a plane with a Shader Graph Material to act as a smog/fog near the ground.
    /// </summary>
    void SpawnCityFog(Rect cityArea)
    {
        Vector3 center = new Vector3(cityArea.x + cityArea.width / 2f, 0.01f, cityArea.y + cityArea.height / 2f);
        GameObject fog = Instantiate(fogPlanePrefab, center, Quaternion.identity);
        fog.transform.localScale = new Vector3(cityArea.width / 10f, 1, cityArea.height / 10f);
        fog.transform.SetParent(this.transform);
    }

    /// <summary>
    /// Places building plots and generates buildings.
    /// </summary>
    void GeneratePlots()
    {
        foreach (Rect plot in plots)
        {
            NeighborhoodProfile profile = GetProfileForPlot(plot) ?? defaultProfile;

            GameObject plotObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plotObj.transform.position = new Vector3(plot.x + plot.width / 2f, 0, plot.y + plot.height / 2f);
            plotObj.transform.localScale = new Vector3(plot.width / 10f, 1, plot.height / 10f);
            plotObj.GetComponent<MeshRenderer>().material = plotMaterial;
            plotObj.transform.SetParent(this.transform);

            // Spawn building
            GameObject building = Instantiate(buildingPrefab, plotObj.transform.position, Quaternion.identity, this.transform);
            building.isStatic = true;

            float rawWidth = plot.width * profile.plotFillFactor;
            float rawDepth = plot.height * profile.plotFillFactor;

            int widthUnits = Mathf.Max(1, Mathf.RoundToInt(rawWidth / buildingGridSize));
            int depthUnits = Mathf.Max(1, Mathf.RoundToInt(rawDepth / buildingGridSize));

            float buildingWidth = widthUnits * buildingGridSize;
            float buildingDepth = depthUnits * buildingGridSize;
            int floors = Random.Range(profile.minFloors, profile.maxFloors + 1);

            // Define simple rectangular base
            List<Vector3> baseCurve = new List<Vector3>
            {
                new Vector3(-buildingWidth / 2f, 0, -buildingDepth / 2f),
                new Vector3(buildingWidth / 2f, 0, -buildingDepth / 2f),
                new Vector3(buildingWidth / 2f, 0, buildingDepth / 2f),
                new Vector3(-buildingWidth / 2f, 0, buildingDepth / 2f)
            };

            // Setup building controller and generator
            BuildingController controller = building.GetComponent<BuildingController>();
            controller.Initialize(buildingWidth, buildingDepth, floors, floorHeight, baseCurve);

            BuildingGenerator generator = building.GetComponent<BuildingGenerator>();
            generator.floorMaterials = profile.wallMaterials;
            generator.roofMaterial = profile.roofMaterial;
            generator.roofPrefabs = profile.roofPrefabs;
            generator.decorationManager = buildingDecorationManager;

            controller.Regenerate();
            SpawnPlotDecorations(plot, buildingWidth, buildingDepth, building.transform.position);
        }
    }

    /// <summary>
    /// Randomly spawns decorative objects around plot perimeter.
    /// </summary>
    void SpawnPlotDecorations(Rect plot, float buildingWidth, float buildingDepth, Vector3 center)
    {
        float sidewalkMarginX = (plot.width - buildingWidth) / 2f;
        float sidewalkMarginZ = (plot.height - buildingDepth) / 2f;
        float edgeSpacing = 4f;

        for (float z = -buildingDepth / 2f; z <= buildingDepth / 2f; z += edgeSpacing)
        {
            TrySpawnDecoration(center + new Vector3(-(buildingWidth / 2f + sidewalkMarginX / 2f), 0, z), 90f);
            TrySpawnDecoration(center + new Vector3((buildingWidth / 2f + sidewalkMarginX / 2f), 0, z), -90f);
        }

        for (float x = -buildingWidth / 2f; x <= buildingWidth / 2f; x += edgeSpacing)
        {
            TrySpawnDecoration(center + new Vector3(x, 0, -(buildingDepth / 2f + sidewalkMarginZ / 2f)), 0f);
            TrySpawnDecoration(center + new Vector3(x, 0, (buildingDepth / 2f + sidewalkMarginZ / 2f)), 180f);
        }
    }

    /// <summary>
    /// Attempts to spawn a decoration at the chosen position if all checks succeed.
    /// </summary>
    void TrySpawnDecoration(Vector3 pos, float outwardYRotation)
    {
        if (Random.value < decorationSpawnChance && plotDecorations.Length > 0)
        {
            PlotDecorationData decoData = plotDecorations[Random.Range(0, plotDecorations.Length)];
            GameObject deco = Instantiate(decoData.prefab, pos, Quaternion.Euler(decoData.localRotationOffset.x, outwardYRotation + decoData.localRotationOffset.y, decoData.localRotationOffset.z), this.transform);
        }
    }

    /// <summary>
    /// Returns the matching neighborhood profile based on zone areas.
    /// </summary>
    NeighborhoodProfile GetProfileForPlot(Rect plot)
    {
        Vector2 center = new Vector2(plot.x + plot.width / 2f, plot.y + plot.height / 2f);
        foreach (CityZone zone in cityZones)
        {
            if (zone.GetRect().Contains(center))
                return zone.profile;
        }
        return defaultProfile;
    }

    /// <summary>
    /// Generates road segments between plots.
    /// </summary>
    void GenerateRoads()
    {
        foreach (var road in roadSegments)
        {
            GameObject roadObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            roadObj.transform.position = new Vector3(road.x + road.width / 2f, -0.01f, road.y + road.height / 2f);
            roadObj.transform.localScale = new Vector3(road.width / 10f, 1, road.height / 10f);
            roadObj.GetComponent<MeshRenderer>().material = roadMaterial;
            roadObj.transform.SetParent(this.transform);

            GenerateRoadLines(road);
        }
    }

    /// <summary>
    /// Generates road markings.
    /// </summary>
    void GenerateRoadLines(Rect road)
    {
        bool vertical = road.width < road.height;

        GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Plane);
        centerLine.transform.position = new Vector3(road.x + road.width / 2f, -0.005f, road.y + road.height / 2f);
        centerLine.transform.localScale = vertical ? new Vector3(0.01f, 1, road.height / 10f) : new Vector3(road.width / 10f, 1, 0.01f);
        centerLine.GetComponent<MeshRenderer>().material = roadLineMaterial;
        centerLine.transform.SetParent(this.transform);
    }

    /// <summary>
    /// Destroys all city objects and clears internal state.
    /// </summary>
    public void ClearCity()
    {
        List<GameObject> toDelete = new List<GameObject>();
        foreach (Transform child in transform)
            toDelete.Add(child.gameObject);

        foreach (var obj in toDelete)
            DestroyImmediate(obj);

        plots.Clear();
        roadSegments.Clear();
    }
}
