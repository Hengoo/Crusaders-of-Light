using System.Collections.Generic;
using UnityEngine;

public class SceneryStructure
{
    public List<SceneryAreaFill> SceneryAreas { get; private set; }
    public TerrainStructure TerrainStructure { get; private set; }

    public SceneryStructure(TerrainStructure terrainStructure, GameObject tree)
    {
        SceneryAreas = new List<SceneryAreaFill>();

        TerrainStructure = terrainStructure;

        Vector2[] array = { new Vector2(0, 0), new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100) };

        SceneryAreas.Add(new SceneryAreaFill(tree, array));

        return;

        foreach (var polygon in TerrainStructure.GetBiomePolygons())
        {
            SceneryAreas.Add(new SceneryAreaFill(tree, polygon));
        }
    }

    public IEnumerable<GameObject> GetSceneryObjects(Terrain terrain)
    {
        var result = new List<GameObject>();
        foreach (var sceneryArea in SceneryAreas)
        {
            var fill = FillSceneryArea(sceneryArea, terrain);
            result.Add(fill);

            var polygon = new GameObject("Poly");
            polygon.transform.parent = fill.transform;
            int count = 0;
            foreach (var point in sceneryArea.Polygon)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = count + " ";
                sphere.GetComponent<Collider>().enabled = false;
                sphere.transform.parent = polygon.transform;
                sphere.transform.position = new Vector3(point.x, 0, point.y);
                sphere.transform.localScale = Vector3.one * 20;
            }

        }
        return result;
    }

    public GameObject FillSceneryArea(SceneryAreaFill sceneryAreaFill, Terrain terrain)
    {
        var result = new GameObject("SceneryAreaFill");
        result.transform.position = Vector3.zero;
        result.transform.rotation = Quaternion.identity;

        var size = sceneryAreaFill.Size;
        PoissonDiskGenerator.minDist = 10;
        PoissonDiskGenerator.sampleRange = size.x > size.y ? size.x : size.y;
        PoissonDiskGenerator.Generate();
        foreach (var sample in PoissonDiskGenerator.ResultSet)
        {
            if (!sample.IsInsidePolygon(sceneryAreaFill.Polygon))
                continue;

            var go = Object.Instantiate(sceneryAreaFill.Prefab);
            var height = terrain.SampleHeight(new Vector3(sample.x, 0, sample.y));
            go.transform.parent = result.transform;
            go.transform.position = new Vector3(sample.x + sceneryAreaFill.BoundMin.x, height, sample.y + sceneryAreaFill.BoundMin.y);
        }

        return result;
    }
}

public class SceneryAreaFill
{
    public readonly Vector2[] Polygon;
    public readonly GameObject Prefab;
    public readonly Vector2 Size;
    public readonly Vector2 BoundMin;
    public readonly Vector2 BoundMax;

    public SceneryAreaFill(GameObject prefab, Vector2[] polygon)
    {
        Prefab = prefab;
        Polygon = polygon;

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

