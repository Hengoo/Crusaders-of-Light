using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerTesting : MonoBehaviour {

    public Spawner SpawnerToTest;

    public int LevelMin = 0;    // Minimum Level of Weapons
    public int LevelMax = 0;    // Maximum Level of Weapons
    public int TestSeed = 0;
    public SpawnerOrder[] TestOrders = new SpawnerOrder[0];
    public int PowerlevelAllowance = 0;
    public Vector3[] Positions = new Vector3[0];
    public Vector3[] Rotations = new Vector3[0]; // Treated as Euler Angles
    public SpawnerFixSpawn[] FixSpawns = new SpawnerFixSpawn[0];
    public TagBiome[] TagsBiome = new TagBiome[0];

    private void Start()
    {
        Random.InitState(TestSeed);
        SpawnerToTest.Initialize(true, PowerlevelAllowance, LevelMin, LevelMax, Positions, Rotations, TagsBiome, TestOrders, FixSpawns);
    }
}
