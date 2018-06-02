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
    public abstract AreaSettings ProduceAreaSettings(Vector2[] centers, Vector2[] polygon, Vector2[] clearPolygons);
}                                                    
public abstract class AreaSettings
{
    public readonly string Name;

    private Vector2[] _centers;
    private Vector2[] _polygon;
    private Vector2[] _clearPolygons;

    public abstract GameObject GenerateAreaScenery();
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
