using System.Collections.Generic;
using TriangleNet.Voronoi;
using UnityEngine;

public class SceneryStructure
{
    public List<SceneryAreaFill> SceneryAreas { get; private set; }
    public TerrainStructure TerrainStructure { get; private set; }
    

    public SceneryStructure(TerrainStructure terrainStructure)
    {
        SceneryAreas = new List<SceneryAreaFill>();

        TerrainStructure = terrainStructure;

        /* Get the biome edges from the terrain structure and create areas to fill with prefabs */
        List<GameObject[]> prefabs;
        List<float> minDistances;
        var polygons = TerrainStructure.GetBiomePolygons(out prefabs, out minDistances);
        for (var i = 0; i < polygons.Count; i++)
        {
            SceneryAreas.Add(new SceneryAreaFill(prefabs[i], polygons[i], minDistances[i]));
        }
    }

    /* Fill all areas with prefabs */
    public IEnumerable<GameObject> FillAllSceneryAreas(Terrain terrain)
    {
        var result = new List<GameObject>();
        foreach (var sceneryArea in SceneryAreas)
        {
            var fill = FillSceneryArea(sceneryArea, terrain);
            result.Add(fill);

            //Debugging
            //var polygon = new GameObject("Poly");
            //polygon.transform.parent = fill.transform;
            //int count = 0;
            //foreach (var point in sceneryArea.Polygon)
            //{
            //    var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    sphere.name = count + " ";
            //    sphere.GetComponent<Collider>().enabled = false;
            //    sphere.transform.parent = polygon.transform;
            //    sphere.transform.position = new Vector3(point.x, 0, point.y);
            //    sphere.transform.localScale = Vector3.one * 20;
            //}

        }
        return result;
    }

    /* Fill an area with prefabs */
    public GameObject FillSceneryArea(SceneryAreaFill sceneryAreaFill, Terrain terrain)
    {
        var result = new GameObject("SceneryAreaFill");
        result.transform.position = Vector3.zero;
        result.transform.rotation = Quaternion.identity;

        if (sceneryAreaFill.Prefabs == null || sceneryAreaFill.Prefabs.Length <= 0)
            return result;

        var size = sceneryAreaFill.Size;
        PoissonDiskGenerator.minDist = sceneryAreaFill.MinDist;
        PoissonDiskGenerator.sampleRange = (size.x > size.y ? size.x : size.y);
        PoissonDiskGenerator.Generate();
        foreach (var sample in PoissonDiskGenerator.ResultSet)
        {
            var point = sample + sceneryAreaFill.BoundMin;
            var height = terrain.SampleHeight(new Vector3(point.x, 0, point.y));
            if (!point.IsInsidePolygon(sceneryAreaFill.Polygon) || height <= (TerrainStructure.BiomeConfiguration.SeaHeight + 0.01f) * terrain.terrainData.size.y)
                continue;

            var go = Object.Instantiate(sceneryAreaFill.Prefabs[Random.Range(0, sceneryAreaFill.Prefabs.Length)]);
            go.transform.position = new Vector3(point.x, height, point.y);
            go.transform.parent = result.transform;
        }

        return result;
    }
}

public class SceneryAreaFill
{
    public readonly Vector2[] Polygon;
    public readonly GameObject[] Prefabs;
    public readonly Vector2 Size;
    public readonly Vector2 BoundMin;
    public readonly Vector2 BoundMax;
    public readonly float MinDist;

    public SceneryAreaFill(GameObject[] prefabs, Vector2[] polygon, float minDist)
    {
        Prefabs = prefabs;
        Polygon = polygon;
        MinDist = minDist;

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var point in polygon)
        {
            minX = Mathf.Min(point.x, minX);
            minY = Mathf.Min(point.y, minY);
            maxX = Mathf.Max(point.x, maxX);
            maxY = Mathf.Max(point.y, maxY);
        }

        BoundMin = new Vector2(minX, minY);
        BoundMax = new Vector2(maxX, maxY);
        Size = new Vector2(maxX - minX, maxY - minY);
    }
}

