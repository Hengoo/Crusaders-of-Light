using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SwarmSpawner : MonoBehaviour {

    //public int DangerLevel = 100;

    // Grouping: Number of Enemies spawned before a new area marker is generated.
    [Header("Grouping:")]
    public int GroupingMin = 5;
    public int GroupingMax = 15;
    private int GroupingCurrent = 0;
    private int GroupingCounter = 0;

    [Header("Frequency:")]
    // Frequency: How fast enemies are spawned after each other:
    public float FrequencyTimerMin = 0.1f;
    public float FrequencyTimerMax = 0.3f;
    private float FrequencyTimerCurrent = 0;
    private float FrequencyTimerCounter = 0;

    [Header("Spawning Cooldown:")]
    public float SpawningCooldownMin = 0.3f;
    public float SpawningCooldownMax = 5f;
    private float SpawningCooldownCounter = 0f;

    [Header("Spawning Radius:")]
    public float SpawnRadiusMin = 15f;  // Min Distance of the AreaMarker to the LightOrb
    public float SpawnRadiusMax = 30f;  // Max Distance of the AreaMarker to the LightOrb
    public float SpawnAreaRadius = 3f;  // Max Distance of spawn position from the AreaMarker

    [Header("Enemies To Spawn:")]
    public EnemySwarm[] EnemyPrefabs = new EnemySwarm[3];
    public float[] EnemyWeight = new float[3];
    private float EnemyWeightTotal = 0;
    private float RolledEnemyWeight = 0;
    private float EnemyWeightCounter = 0;
    private int CurrentEnemyPrefabID = 0;

    [Header("Spawned Enemies:")]
    public int SpawnedEnemiesMaxNumber = 300;
    private EnemySwarm[] SpawnedEnemies;
    private int SpawnedEnemiesCounter = 0;
    private int SpawnedEnemiesIDCounter = 0;
    public int LayerMask = 0;

    [Header("Player Characters:")]
    public Character[] Players = new Character[0];

    [Header("Terrain and Navmesh:")]
    public Terrain Terr;

    private Vector3 spawnAreaMarker = Vector3.zero;
    private int spawnAreaMarkerMaxTries = 10;
    private int spawnAreaMarkerTryCounter = 0;

    private Vector3 spawnPos = Vector3.zero;
    private int spawnPosMaxTries = 10;
    private int spawnPosTryCounter = 0;

    private NavMeshHit hit;

    private void Start()
    {
        CalculateTotalWeight();

        SpawnedEnemies = new EnemySwarm[SpawnedEnemiesMaxNumber];
        LayerMask = 1 << LayerMask;
    }

    private void Update()
    {
        UpdateSpawningCooldown();
        SpawnEnemy();
        UpdateAllSwarmlings();
    }

    private void SpawnEnemy()
    {
        if (FrequencyTimerCounter < FrequencyTimerCurrent)
        {
            FrequencyTimerCounter += Time.deltaTime;
            return;
        }

        if (SpawningCooldownCounter > 0)
        {
            return;
        }

        if (SpawnedEnemiesCounter >= SpawnedEnemiesMaxNumber)
        {
            return;
        }

        // Decide the next enemy type to spawn:
        RandomlyDecideNextEnemyType();

        // Decide the next enemy spawn position and check if it was possible to generate such a position:
        if (!RandomlyDecideNextEnemySpawnPosition())
        {
            return;
        }

        if (!CalculateNextSpawnNumber())
        {
            return;
        }

        SpawnedEnemies[SpawnedEnemiesIDCounter] = Instantiate(EnemyPrefabs[CurrentEnemyPrefabID], spawnPos, EnemyPrefabs[CurrentEnemyPrefabID].transform.rotation);
        SpawnedEnemies[SpawnedEnemiesIDCounter].InitializeSwarmling(this, SpawnedEnemiesIDCounter, Players, LayerMask);

        // At this point an enemy was succesfully spawned:
        SpawnedEnemiesCounter++;
        FrequencyTimerCounter = 0;
        FrequencyTimerCurrent = Random.Range(FrequencyTimerMin, FrequencyTimerMax);
    }

    private void RandomlyDecideNextEnemyType()
    {
        RolledEnemyWeight = Random.Range(0, EnemyWeightTotal);
        EnemyWeightCounter = 0;

        for (int i = 0; i < EnemyWeight.Length; i++)
        {
            EnemyWeightCounter += EnemyWeight[i];
            if (RolledEnemyWeight < EnemyWeightCounter)
            {
                CurrentEnemyPrefabID = i;
                return;
            }
        }
    }

    private void CalculateTotalWeight() // This needs to be called at start once and if the Enemy Weights change at runtime!
    {
        for (int i = 0; i < EnemyWeight.Length; i++)
        {
            EnemyWeightTotal += EnemyWeight[i];
        }
    }

    private bool RandomlyDecideNextEnemySpawnPosition()
    {
        if (GroupingCounter < GroupingCurrent)
        {
            GroupingCounter++;
            

            if (GroupingCounter >= GroupingCurrent)
            {
                SpawningCooldownCounter = Random.Range(SpawningCooldownMin, SpawningCooldownMax);
            }

            return GenerateSpawnPosition(); 
        }
        else
        {
            if (GenerateSpawnMarkerPosition())
            {
                GroupingCounter = 1;
                GroupingCurrent = Random.Range(GroupingMin, GroupingMax);
                
                return GenerateSpawnPosition();
            }
        }
        return false;
    }

    private bool GenerateSpawnPosition()
    {
        if (spawnPosTryCounter >= spawnPosMaxTries)
        {
            spawnPosTryCounter = 0;
            return false;
        }
        spawnPosTryCounter++;

        spawnPos = Vector3.forward * Random.Range(0, SpawnAreaRadius);
        spawnPos = Quaternion.Euler(0, Random.Range(0, 360), 0) * spawnPos;
        spawnPos += spawnAreaMarker;

        spawnPos.y = Terr.SampleHeight(spawnPos);

        NavMesh.SamplePosition(spawnPos, out hit, 3, NavMesh.AllAreas);

        spawnPos = hit.position;

        if (spawnPos.x == Mathf.Infinity)
        {
            return GenerateSpawnPosition();           
        }
        else
        {
            return true;
        }
    }

    private bool GenerateSpawnMarkerPosition()
    {
        if (spawnAreaMarkerTryCounter >= spawnAreaMarkerMaxTries)
        {
            spawnAreaMarkerTryCounter = 0;
            return false;
        }
        spawnAreaMarkerTryCounter++;

        spawnAreaMarker = Vector3.forward * Random.Range(SpawnRadiusMin, SpawnRadiusMax);
        spawnAreaMarker = Quaternion.Euler(0, Random.Range(0, 360), 0) * spawnAreaMarker;
        spawnAreaMarker += gameObject.transform.position;

        spawnAreaMarker.y = Terr.SampleHeight(spawnAreaMarker);

        NavMesh.SamplePosition(spawnAreaMarker, out hit, 4, NavMesh.AllAreas);

        spawnAreaMarker = hit.position;

        if (spawnAreaMarker.x >= Mathf.Infinity)
        {
            return GenerateSpawnMarkerPosition();
        }
        else
        {
            return true;
        }
    }

    public void UpdateSpawningCooldown()
    {
        if (SpawningCooldownCounter > 0)
        {
            SpawningCooldownCounter -= Time.deltaTime;
        }
    }

    // Searches the next free slot in the Spawned Enemies array and returns true if a slot was found.
    // Does no search if no slot is free.
    // Worst Case: Search the entire array once.
    public bool CalculateNextSpawnNumber()
    {
        if (SpawnedEnemiesCounter >= SpawnedEnemiesMaxNumber)
        {
            return false;
        }

        while(SpawnedEnemies[SpawnedEnemiesIDCounter] != null)
        {
            SpawnedEnemiesIDCounter = (SpawnedEnemiesIDCounter + 1) % SpawnedEnemiesMaxNumber;
            //Debug.Log("Modulo: " + SpawnedEnemiesIDCounter);
        }

        return true;
    }

    public void SwarmlingDied(int SwarmlingID)
    {
        SpawnedEnemies[SwarmlingID] = null;
        SpawnedEnemiesCounter--;
    }

    private void UpdateAllSwarmlings()
    {
        for (int i = 0; i < SpawnedEnemies.Length; i++)
        {
            if (SpawnedEnemies[i])
            {
                SpawnedEnemies[i].SwarmlingUpdate();
            }
        }
    }
}
