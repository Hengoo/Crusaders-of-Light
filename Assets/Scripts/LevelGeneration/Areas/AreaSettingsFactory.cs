using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

public abstract class AreaSettingsFactory : ScriptableObject, IComparable
{
    public GameObject ArenaTriggerPrefab;
    public GameObject FogGatePrefab;

    public abstract Graph<AreaSegment> GetPatternGraph();

    public abstract AreaSettings[] ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon);

    public int CompareTo(object obj)
    {
        return GetPatternGraph().GetAllNodeIDs().Length - ((AreaSettingsFactory)obj).GetPatternGraph().GetAllNodeIDs().Length;
    }
}