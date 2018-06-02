using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

//-------------------------------------------------------------
// Areas
//-------------------------------------------------------------
public abstract class AreaSettingsFactory : ScriptableObject
{
    public abstract Graph<AreaSegment> GetPatternGraph();
    public abstract AreaSettings ProduceAreaSettings(IEnumerable<Vector2> centers, Graph<Vector2[]> polygonGraph, IEnumerable<Vector2[]> clearPolygons);
}                                                    
public abstract class AreaSettings
{
    public string Name = "Area";
    public List<PoissonDiskFillData> PoissonData = new List<PoissonDiskFillData>();

    protected Vector2[] Centers;
    protected Graph<Vector2[]> PolygonGraph;
    protected Vector2[][] ClearPolygons;

    public abstract GameObject GenerateAreaScenery(Terrain terrain);
}

public class AreaSegment : IEquatable<AreaSegment>
{
    public enum EAreaSegmentType
    {
        Empty,
        Border,
        Start,
        Boss,
        Special,
        MainPath,
        SidePath
    }

    public enum EAreaSegmentEdgeType
    {
        NonNavigable,
        MainPath,
        SidePath,
        BossInnerPath,
        SpecialInnerPath
    }

    public EAreaSegmentType Type;

    public AreaSegment(EAreaSegmentType type)
    {
        Type = type;
    }

    public bool Equals(AreaSegment other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == this.GetType() && Equals((AreaSegment) obj);
    }

    public override int GetHashCode()
    {
        return 1;
    }

    public static bool operator ==(AreaSegment left, AreaSegment right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AreaSegment left, AreaSegment right)
    {
        return !Equals(left, right);
    }
}
