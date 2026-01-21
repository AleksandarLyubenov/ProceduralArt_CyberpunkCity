using System.Collections.Generic;
using UnityEngine;

public class ProceduralCityGenerator : MonoBehaviour
{
    [Header("City Size")]
    public float cityWidth = 100f;
    public float cityDepth = 100f;

    [Header("Plot Settings")]
    public float maxPlotWidth = 30f;
    public float maxPlotDepth = 30f;
    public float minPlotWidth = 10f;
    public float minPlotDepth = 10f;
    public float roadWidth = 4f;

    [Header("Buildings")]
    public GameObject buildingPrefab;
    public float buildingGridSize = 1f;
    public float plotFillFactor = 0.8f;
    public float floorHeight = 2f;
    public int minFloors = 5;
    public int maxFloors = 15;

    [Header("Materials")]
    public Material roadMaterial;
    public Material plotMaterial;
    public Material[] buildingMaterials;


    private List<Rect> plots = new List<Rect>();

    void Start()
    {
        GenerateCity();
    }

    [ContextMenu("Generate City")]
    public void GenerateCity()
    {
        ClearCity();
        Rect initialRect = new Rect(-cityWidth / 2f, -cityDepth / 2f, cityWidth, cityDepth);
        Subdivide(initialRect);
        GeneratePlots();
        GenerateRoads();
    }

    void Subdivide(Rect area)
    {
        // Force subdivision if area exceeds max plot size
        if (area.width > maxPlotWidth || area.height > maxPlotDepth)
        {
            bool splitHorizontally;

            // Decide axis to split
            if (area.width > maxPlotWidth && area.height > maxPlotDepth)
                splitHorizontally = (area.width < area.height);
            else
                splitHorizontally = (area.height > area.width);

            if (splitHorizontally)
            {
                // Horizontal split
                float minSplit = minPlotDepth + roadWidth / 2f;
                float maxSplit = area.height - minPlotDepth - roadWidth / 2f;

                if (maxSplit > minSplit)
                {
                    float splitY = Random.Range(minSplit, maxSplit);
                    Rect bottom = new Rect(area.x, area.y, area.width, splitY - roadWidth / 2f);
                    Rect top = new Rect(area.x, area.y + splitY + roadWidth / 2f, area.width, area.height - splitY - roadWidth / 2f);
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
                // Vertical split
                float minSplit = minPlotWidth + roadWidth / 2f;
                float maxSplit = area.width - minPlotWidth - roadWidth / 2f;

                if (maxSplit > minSplit)
                {
                    float splitX = Random.Range(minSplit, maxSplit);
                    Rect left = new Rect(area.x, area.y, splitX - roadWidth / 2f, area.height);
                    Rect right = new Rect(area.x + splitX + roadWidth / 2f, area.y, area.width - splitX - roadWidth / 2f, area.height);
                    Subdivide(left);
                    Subdivide(right);
                }
                else
                {
                    plots.Add(area);
                }
            }
        }
        else
        {
            // Area is within max plot size -> keep as plot
            plots.Add(area);
        }
    }

    void GeneratePlots()
    {
        foreach (Rect plot in plots)
        {
            GameObject plotObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plotObj.transform.position = new Vector3(plot.x + plot.width / 2f, 0, plot.y + plot.height / 2f);
            plotObj.transform.localScale = new Vector3(plot.width / 10f, 1, plot.height / 10f);
            plotObj.GetComponent<MeshRenderer>().material = plotMaterial;

            GameObject building = Instantiate(buildingPrefab);
            building.transform.position = new Vector3(plot.x + plot.width / 2f, 0, plot.y + plot.height / 2f);

            float rawWidth = plot.width * plotFillFactor;
            float rawDepth = plot.height * plotFillFactor;

            int widthUnits = Mathf.Max(1, Mathf.RoundToInt(rawWidth / buildingGridSize));
            int depthUnits = Mathf.Max(1, Mathf.RoundToInt(rawDepth / buildingGridSize));

            float buildingWidth = widthUnits * buildingGridSize;
            float buildingDepth = depthUnits * buildingGridSize;

            int floors = Random.Range(minFloors, maxFloors + 1);
            float buildingHeight = floors * floorHeight;

            List<Vector3> baseCurve = new List<Vector3>();
            float halfWidth = buildingWidth / 2f;
            float halfDepth = buildingDepth / 2f;

            baseCurve.Add(new Vector3(-halfWidth, 0, -halfDepth));
            baseCurve.Add(new Vector3(halfWidth, 0, -halfDepth));
            baseCurve.Add(new Vector3(halfWidth, 0, halfDepth));
            baseCurve.Add(new Vector3(-halfWidth, 0, halfDepth));

            BuildingController controller = building.AddComponent<BuildingController>();
            controller.Initialize(buildingWidth, buildingDepth, floors, floorHeight, baseCurve);
            controller.Regenerate();
        }
    }

    void GenerateRoads()
    {
        GameObject roads = GameObject.CreatePrimitive(PrimitiveType.Plane);
        roads.transform.position = new Vector3(0, -0.01f, 0);
        roads.transform.localScale = new Vector3(cityWidth / 10f, 1, cityDepth / 10f);
        roads.GetComponent<MeshRenderer>().material = roadMaterial;
    }

    void ClearCity()
    {
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
        plots.Clear();
    }
}
