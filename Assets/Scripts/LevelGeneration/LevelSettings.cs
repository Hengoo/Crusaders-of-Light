using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

//-------------------------------------------------------------
// Biomes & Terrain
//-------------------------------------------------------------
[Serializable]
public class GlobalSettings
{
    [Range(16, 1024)] public int HeightMapResolution = 512;
    [Range(16, 1024)] public float MapSize = 512;
    [Range(1, 1024)] public float MapHeight = 80;
    [Range(10, 1000)] public int VoronoiSamples = 30;
    [Range(0, 1f)] public float MaxHeight = 1;
    [Range(0, 1f)] public float SeaHeight = 0.15f;
    [Range(0, 50f)] public float EdgeNoise = 8f;
    [Range(0, 20)] public int LloydRelaxation = 5;
    [Range(1, 8)] public int Octaves = 3;
    public BiomeSettings BorderBiome;
    public Material WaterMaterial;
    public GameObject CoastBlocker;
    public GameObject CoastBlockerPole;
    [Range(0.01f, 20)] public float CoastBlockerLength = 3.5f;
    [Range(0f, 50f)] public float CoastInlandOffset = 20f;
    public GameObject AreaBlocker;
    [Range(0.01f, 20f)] public float AreaBlockerLength = 2.1f;
    public SplatPrototypeSerializable RoadSplatPrototype;
    [Range(0, 5)] public int OverallSmoothing = 2;
    public bool SmoothEdges = true;
    [Range(0, 20)] public int EdgeWidth = 3;
    [Range(0, 20)] public int SquareSize = 2;
}

[CreateAssetMenu(fileName = "Biome Settings", menuName = "Terrain/Biome Settings")]
public class BiomeSettings : ScriptableObject
{
    public string UniqueName;
    public BiomeHeightParameters HeightParameters;
    public SplatPrototypeSerializable Splat;

    public BiomeSettings(BiomeHeightParameters heightParameters, bool notNavigable)
    {
        HeightParameters = heightParameters;
    }
}

[Serializable]
public class BiomeHeightParameters
{
    [Range(0, 1)] public float Persistence = 0.3f;
    [Range(0, 1)] public float LocalMax = 0.6f;
    [Range(0, 1)] public float LocalMin = 0.5f;
    [Range(0, 20)] public float Lacunarity = 2f;
    [Range(1f, 100f)] public float Scale = 10f;

    public BiomeHeightParameters(float lacunarity, float persistence, float localMax, float localMin, float scale)
    {
        Lacunarity = lacunarity;
        LocalMax = localMax;
        Scale = scale;
        LocalMin = localMin > localMax ? localMax : localMin;
        Persistence = Mathf.Clamp01(persistence);
    }
}


//-------------------------------------------------------------
// Areas
//-------------------------------------------------------------
[CreateAssetMenu(fileName = "Area Configuration", menuName = "Terrain/Area Configuration")]
public class AreaConfiguration : ScriptableObject
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

    public EAreaSegmentType Type;
    public readonly Vector2 Center;

    public AreaSegment(EAreaSegmentType type)
    {
        Center = Vector2.zero;
        Type = type;
    }

    public AreaSegment(Vector2 center, EAreaSegmentType type)
    {
        Center = center;
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
        if (obj.GetType() != this.GetType()) return false;
        return Equals((AreaSegment) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int) Type * 397) ^ Center.GetHashCode();
        }
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
    public AreaConfiguration AreaConfiguration;
}
