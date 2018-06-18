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
    public Material TerrainMaterial;
    public SplatPrototypeSerializable Splat;
    public GameObject Blocker;
    public GameObject BlockerPole;
    [Range(0.01f, 20f)] public float BlockerLength = 2.1f;
    [Range(0.01f, 80f)]  public float BlockerAngleLimit = 80;
    public Vector3 BlockerPositionNoise = Vector3.zero;
    public Vector3 BlockerScaleNoise = Vector3.zero;

    [Header("Area Settings")]
    public AreaSettingsFactory[] BossAreas;
    public AreaSettingsFactory[] ChestAreas;
    public AreaSettingsFactory[] PathAreas;

    [Header("Path Settings")]
    public SplatPrototypeSerializable MainPathSplatPrototype;
    public SplatPrototypeSerializable SidePathSplatPrototype;
    [Range(1, 20)] public int MainPathSplatSize = 2;
    [Range(1, 20)] public int SidePathSplatSize = 1;
    [Range(1, 20)] public float PathHalfWidth = 5;
    [Range(1, 50)] public float PathBezierSegmentSize = 15;
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
