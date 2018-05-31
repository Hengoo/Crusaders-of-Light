using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-------------------------------------------------------------
// Biomes & Terrain
//-------------------------------------------------------------

[CreateAssetMenu(fileName = "BiomeSettings", menuName = "Terrain/Biome Settings")]
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
