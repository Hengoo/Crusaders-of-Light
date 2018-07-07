using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class AreaSettings
{
    public string Name = "Area";
    public readonly List<PoissonDiskFillData> PoissonDataList = new List<PoissonDiskFillData>();

    public GameObject ArenaTriggerPrefab;
    public GameObject FogGatePrefab;
    public Graph<AreaData> AreaDataGraph { get; protected set; }
    public List<Vector2[]> ClearPolygons { get; protected set; }
    public List<Vector2> BorderPolygon { get; protected set; }

    public abstract GameObject GenerateAreaScenery(Terrain terrain);

    protected GameObject PlaceArena(AreaData data, Terrain terrain)
    {
        var center = new Vector3(data.Center.x, 0, data.Center.y);
        center += new Vector3(0, terrain.SampleHeight(center), 0);

        var arena = Object.Instantiate(ArenaTriggerPrefab, center, Quaternion.identity);
        var gatesList = new List<GameObject>();

        foreach (var gateLine in GetGateLines(data.Polygon))
        {
            // Generate gate
            var line = gateLine[0] - gateLine[1];
            var gatePosition2D = (gateLine[0] + gateLine[1]) / 2;
            var gatePosition = new Vector3(gatePosition2D.x, 0, gatePosition2D.y);
            var gate = Object.Instantiate(FogGatePrefab);
            var shape = gate.GetComponent<ParticleSystem>().shape;
            shape.scale += new Vector3(0, 0, line.magnitude - 1);
            gate.GetComponent<BoxCollider>().size += new Vector3(0, 0, line.magnitude - 1);
            gate.GetComponent<NavMeshObstacle>().size += new Vector3(0, 0, line.magnitude - 1);
            gate.transform.position = new Vector3(gatePosition.x, terrain.SampleHeight(gatePosition), gatePosition.z);
            gate.transform.rotation = Quaternion.LookRotation(new Vector3(line.x, 0, line.y), Vector3.up);
            gate.transform.parent = arena.transform;

            // Add to list
            gatesList.Add(gate);
        }

        // Initialize component
        arena.GetComponent<AreaArenaTrigger>().Initialize(gatesList, data.Polygon.OffsetToCenter(data.Center, 5).ToArray());

        return arena;
    }

    private List<Vector2[]> GetGateLines(Vector2[] borderPolygon)
    {
        List<Vector2[]> result = new List<Vector2[]>();
        for (int i = 0; i < borderPolygon.Length; i++)
        {
            var p0 = borderPolygon[i];
            var p1 = borderPolygon[(i + 1) % borderPolygon.Length];
            var center = (p1 + p0) / 2;

            foreach (var clearPolygon in ClearPolygons)
            {
                if (center.IsInsidePolygon(clearPolygon))
                {
                    result.Add(new []{ p0, p1});
                }
            }
        }
        return result;
    }
}

public class AreaData
{
    public Vector2 Center;
    public AreaSegment Segment;
    public Vector2[] Polygon;
    public List<Vector2[]> GateLines;
    public List<Vector2[]> Paths;
}