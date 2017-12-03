using System.Collections.Generic;
using UnityEngine;

public class SceneryStructure
{
    public List<SceneryArea> SceneryAreas { get; private set; }
    public TerrainStructure TerrainStructure { get; private set; }

    public SceneryStructure(TerrainStructure terrainStructure, GameObject tree)
    {
        SceneryAreas = new List<SceneryArea>();

        TerrainStructure = terrainStructure;

        foreach (var polygon in TerrainStructure.GetBiomePolygons())
        {
            SceneryAreas.Add(new SceneryArea(tree, polygon));
        }
    }

    public IEnumerable<GameObject> GetSceneryObjects(Terrain terrain)
    {
        var result = new List<GameObject>();
        foreach (var sceneryArea in SceneryAreas)
        {
            var fill = FillSceneryArea(sceneryArea, terrain);
            result.Add(fill);


            var polyGO = new GameObject("Poly");
            polyGO.transform.parent = fill.transform;
            int count = 0;
            foreach (var point in sceneryArea.Polygon)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = count + " ";
                sphere.GetComponent<Collider>().enabled = false;
                sphere.transform.parent = polyGO.transform;
                sphere.transform.position = new Vector3(point.x, 0, point.y);
                sphere.transform.localScale = Vector3.one * 20;
            }

        }

        return result;
    }

    public GameObject FillSceneryArea(SceneryArea sceneryArea, Terrain terrain)
    {
        var result = new GameObject("SceneryArea");
        result.transform.position = Vector3.zero;
        result.transform.rotation = Quaternion.identity;

        var size = sceneryArea.Size;
        PoissonDiskGenerator.minDist = 10;
        PoissonDiskGenerator.sampleRange = size.x > size.y ? size.x : size.y;
        PoissonDiskGenerator.Generate();
        foreach (var sample in PoissonDiskGenerator.ResultSet)
        {
            if (!sample.IsInsidePolygon(sceneryArea.Polygon))
                continue;

            var go = Object.Instantiate(sceneryArea.Prefab);
            var height = terrain.SampleHeight(new Vector3(sample.x, 0, sample.y));
            go.transform.parent = result.transform;
            go.transform.position = new Vector3(sample.x + sceneryArea.BoundMin.x, height, sample.y + sceneryArea.BoundMin.y);
        }

        return result;
    }
}

public class SceneryArea
{
    public readonly Vector2[] Polygon;
    public readonly GameObject Prefab;
    public readonly Vector2 Size;
    public readonly Vector2 BoundMin;
    public readonly Vector2 BoundMax;

    public SceneryArea(GameObject prefab, Vector2[] polygon)
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

