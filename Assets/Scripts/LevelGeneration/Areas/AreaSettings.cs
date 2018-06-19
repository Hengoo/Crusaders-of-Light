using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class AreaSettings
{
    public string Name = "Area";
    public List<PoissonDiskFillData> PoissonDataList = new List<PoissonDiskFillData>();

    public Graph<AreaData> AreaDataGraph { get; protected set; }
    public List<Vector2[]> ClearPolygons { get; protected set; }
    public List<Vector2> BorderPolygon { get; protected set; }

    public abstract GameObject GenerateAreaScenery(Terrain terrain);
}

public class AreaData
{
    public Vector2 Center;
    public AreaSegment Segment;
    public Vector2[] Polygon;
    public List<Vector2[]> BlockerLines;
    public List<Vector2[]> Paths;
}