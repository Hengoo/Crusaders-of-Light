using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

//-------------------------------------------------------------
// Areas
//-------------------------------------------------------------
[CreateAssetMenu(fileName = "AreaSettings", menuName = "Terrain/Area Settings")]
public class AreaSettings : ScriptableObject
{
    public Vector2[] AreaPolygon;
    public GameObject[] Prefabs;
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
    public Guid uniqueId;

    public AreaSegment(EAreaSegmentType type)
    {
        Type = type;
        uniqueId = Guid.NewGuid();
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

 public class Area
{
    public readonly List<AreaSegment> AreaSegments = new List<AreaSegment>();
    public AreaSettings AreaSettings;
}
