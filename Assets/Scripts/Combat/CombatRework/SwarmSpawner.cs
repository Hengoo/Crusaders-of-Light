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

    public bool SpawningRunning = false;



    [Header("Spawning Cooldown:")]
    public float SpawningCooldownMin = 0.3f;
    public float SpawningCooldownMax = 5f;
    private float SpawningCooldownCounter = 0f;

    [Header("Spawning Radius:")]
    public float SpawnRadiusMin = 15f;  // Min Distance of the AreaMarker to the LightOrb
    public float SpawnRadiusMax = 30f;  // Max Distance of the AreaMarker to the LightOrb
    public float SpawnAreaRadius = 3f;  // Max Distance of spawn position from the AreaMarker

    private Vector2[] SpawnAreaPolygon = new Vector2[0];

    [Header("Enemies To Spawn:")]
    public EnemySwarm[] EnemyPrefabs = new EnemySwarm[3];
    public float[] EnemyWeight = new float[3];
    private float EnemyWeightTotal = 0;
    private float RolledEnemyWeight = 0;
    private float EnemyWeightCounter = 0;
    private int CurrentEnemyPrefabID = 0;

    private float SwarmlingHealthFactor = 1;

    public float DifficultySwarmlingHealthFactor = 1;

    [Header("Spawned Enemies:")]
    public int SpawnedEnemiesMaxNumber = 300;
    private EnemySwarm[] SpawnedEnemies;
    private int SpawnedEnemiesCounter = 0;
    private int SpawnedEnemiesIDCounter = 0;
    public int LayerMask = 0;

    [Header("Player Characters:")]
    public CharacterPlayer[] Players = new CharacterPlayer[0];
    public float GlobalHealFactor = 1f;

    private Terrain terrain;
    private Vector3 spawnAreaMarker = Vector3.zero;
    private int spawnAreaMarkerMaxTries = 10;
    private int spawnAreaMarkerTryCounter = 0;

    private Vector3 spawnPos = Vector3.zero;
    private Vector2 spawnPos2D = Vector2.zero;
    private int spawnPosMaxTries = 10;
    private int spawnPosTryCounter = 0;

    private NavMeshHit hit;

    private NavMeshPath NavPath;

    [Header("Spawn Direction:")]
    public LightWispMovement WispMovement;

    [Header("Difficulty Scaling:")]
    public float DifficultyScaleSpawnNumber = 0.5f;     // (Diff - 1) * Scale + 1. Example: Diff is 1.5, Scale is 0.5 => BaseNumber * 1.25
    public float DifficultyScaleHealthFactor = 0.2f;    // Example: Diff is 1.5, Scale is 0.2 => BaseHealth * 1.1
    private float TotalHealthFactor = 1;

    [Header("Arena Rules:")]
    public float ArenaKillPercentageToWin = 0.9f;
    private int ArenaWinThreshold = 0;
    private bool ArenaBossArea = false;

    public int SpawnNumberMaxTotalBase = 50;                    // Base value of how many should spawn in an arena.
    public int SpawnNumberMaxTotalRemaining = 0;               // Counter for how many still need to spawn in current arena.

    private AreaArenaTrigger CurrentArenaTrigger;

    [Header("Boss Beetles:")]
    public float BossBeetlesHealOnDeathFactor = 3;
    public float BossBeetlesHealthFactor = 3;

    private void Start()
    {
        if (GameController.Instance)
        {
            SpawnedEnemiesMaxNumber = GameController.Instance.GetMaxNumberSwarmlings();
            SwarmlingHealthFactor = GameController.Instance.GetSwarmlingHealthFactor();
        }

        if (!WispMovement)
        {
            WispMovement = gameObject.GetComponent<LightWispMovement>();
        }

        CalculateTotalWeight();

        SpawnedEnemies = new EnemySwarm[SpawnedEnemiesMaxNumber];
        LayerMask = 1 << LayerMask;
    }

    private void Update()
    {
        if (SpawningRunning)
        {
            UpdateSpawningCooldown();
            SpawnEnemy();
        }

        UpdateAllSwarmlings();
    }

    private void FixedUpdate()
    {
        PPHelper.Instance.UpdateBuffer(SpawnedEnemies);
    }

    public void InitializeSwarmSpawner(CharacterPlayer[] PlayerCharacters, int NumberActivePlayers)
    {
        Players = new CharacterPlayer[NumberActivePlayers];

        for (int i = 0; i < Players.Length; i++)
        {
            if (PlayerCharacters[i])
            {
                Players[i] = PlayerCharacters[i];
            }
        }

        NavPath = new NavMeshPath();
    }

    public void ArenaStartSpawning(AreaArenaTrigger NewArenaTrigger, Vector2[] AreaPolygon, bool IsBossArena)
    {
        CurrentArenaTrigger = NewArenaTrigger;

        CalculateArenaSpawnNumber();
        CalculateTotalHealthFactor();
        SpawnAreaPolygon = AreaPolygon;

        SpawningCooldownCounter = 3;
        
        if (!IsBossArena)
        {
            SpawningRunning = true;
        }
        else
        {
            ArenaBossArea = true;
        }
    }

    public void ArenaSetInBossArea(bool state)
    {
        ArenaBossArea = state;
    }

    public void ArenaFinishedBossFight()
    {
        ArenaBossArea = false;
    }

    public void ArenaInBossAreaStart(bool SpecialBeetles)
    {
        SpawningRunning = true;
    }

    private void ArenaFinished()
    {
        if (CurrentArenaTrigger)
        {
            SpawningRunning = false; // Should not be needed, but just to make sure!
            DestroyAllSwarmlings();

            SpawnAreaPolygon = new Vector2[0];

            GameController.Instance.DifficultyFinishedArena();

            CurrentArenaTrigger.OpenArena();
            CurrentArenaTrigger = null;
        }
    }

    private void SpawnEnemy()
    {
        if (SpawnNumberMaxTotalRemaining <= 0)
        {
            SpawningRunning = false;
            return;
        }

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
        SpawnedEnemies[SpawnedEnemiesIDCounter].InitializeSwarmling(this, SpawnedEnemiesIDCounter, Players, LayerMask, SwarmlingHealthFactor);

        // At this point an enemy was succesfully spawned:
        SpawnedEnemiesCounter++;
        FrequencyTimerCounter = 0;
        FrequencyTimerCurrent = Random.Range(FrequencyTimerMin, FrequencyTimerMax);
        SpawnNumberMaxTotalRemaining--;
    }

    public void SpawnEnemyBatch(int EnemyNumber, EnemySwarm EnemyPrefab, Vector3 AreaCenter, float AreaRadius)
    {
        for (int i = 0; i < EnemyNumber; i++)
        {
            if (SpawnedEnemiesCounter >= SpawnedEnemiesMaxNumber)
            {
                return;
            }

            // Decide the next enemy spawn position and check if it was possible to generate such a position:
            if (!GenerateSpawnPosition(AreaCenter, AreaRadius))
            {
                return;
            }

            if (!CalculateNextSpawnNumber())
            {
                return;
            }

            SpawnedEnemies[SpawnedEnemiesIDCounter] = Instantiate(EnemyPrefab, spawnPos, EnemyPrefab.transform.rotation);
            SpawnedEnemies[SpawnedEnemiesIDCounter].InitializeSwarmling(this, SpawnedEnemiesIDCounter, Players, LayerMask, SwarmlingHealthFactor);

            // At this point an enemy was succesfully spawned:
            SpawnedEnemiesCounter++;
        }      
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
        /* if (spawnPosTryCounter >= spawnPosMaxTries)
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

         NavMesh.CalculatePath(spawnPos, transform.position, NavMesh.AllAreas, NavPath);

         if (NavPath.status == NavMeshPathStatus.PathInvalid || NavPath.status == NavMeshPathStatus.PathPartial)
         {
             return GenerateSpawnPosition();
         }
         return true;*/
        return GenerateSpawnPosition(spawnAreaMarker, SpawnAreaRadius);
    }

    private bool GenerateSpawnPosition(Vector3 AreaCenter, float AreaRadius)
    {
        if (spawnPosTryCounter >= spawnPosMaxTries)
        {
            spawnPosTryCounter = 0;
            return false;
        }

        spawnPosTryCounter++;

        spawnPos = Vector3.forward * Random.Range(0, AreaRadius);
        spawnPos = Quaternion.Euler(0, Random.Range(0, 360), 0) * spawnPos;
        spawnPos += AreaCenter;

        spawnPos.y = terrain.SampleHeight(spawnPos);

        NavMesh.SamplePosition(spawnPos, out hit, 3, NavMesh.AllAreas);

        spawnPos = hit.position;

        // If not on Terrain/Navmesh, try again:
        if (spawnPos.x == Mathf.Infinity)
        {
            return GenerateSpawnPosition(AreaCenter, AreaRadius);
        }
        // -> It is on the terrain:

        // If not in Spawn Polygon, try again:
        spawnPos2D = new Vector2(spawnPos.x, spawnPos.z);
        if (SpawnAreaPolygon.Length > 0 && !spawnPos2D.IsInsidePolygon(SpawnAreaPolygon))
        {
            return GenerateSpawnPosition(AreaCenter, AreaRadius);
        }
        // -> It is in the polygon:

        // Check if enemy can reach Spawner (Wisp), if not, try again:
        NavMesh.CalculatePath(spawnPos, transform.position, NavMesh.AllAreas, NavPath);

        if (NavPath.status == NavMeshPathStatus.PathInvalid || NavPath.status == NavMeshPathStatus.PathPartial)
        {
            return GenerateSpawnPosition(AreaCenter, AreaRadius);
        }
        // -> Enemy could reach Spawner:

        return true;
    }

    private bool GenerateSpawnMarkerPosition()
    {
        if (spawnAreaMarkerTryCounter >= spawnAreaMarkerMaxTries)
        {
            spawnAreaMarkerTryCounter = 0;
            return false;
        }
        spawnAreaMarkerTryCounter++;

        //spawnAreaMarker = Vector3.forward * Random.Range(SpawnRadiusMin, SpawnRadiusMax);
        //spawnAreaMarker = Quaternion.Euler(0, Random.Range(0, 360), 0) * spawnAreaMarker;

        spawnAreaMarker = WispMovement.GetPlayerHeading() * Random.Range(SpawnRadiusMin, SpawnRadiusMax);

        spawnAreaMarker += gameObject.transform.position;

        spawnAreaMarker.y = terrain.SampleHeight(spawnAreaMarker);

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

        if (SpawnNumberMaxTotalRemaining <= 0
            && SpawnedEnemiesCounter <= ArenaWinThreshold
            && !ArenaBossArea)
        {
            ArenaFinished();
        }
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

    public void DestroyAllSwarmlings()
    {
        for (int i = 0; i < SpawnedEnemies.Length; i++)
        {
            if (SpawnedEnemies[i])
            {
                SpawnedEnemies[i].SwarmlingSuicide();
            }
        }
    }

    public void SwitchSpawningOnOff(bool state)
    {
        SpawningRunning = state;
    }
    
    public void EnteredBossArena()
    {
        SwitchSpawningOnOff(false);
        DestroyAllSwarmlings();
    }

    public void SetTerrain(Terrain NewTerrain)
    {
        terrain = NewTerrain;
    }

    public void CalculateTotalHealthFactor()
    {
        TotalHealthFactor = SwarmlingHealthFactor * (((GameController.Instance.GetCurrentDifficultyFactor() - 1) * DifficultyScaleHealthFactor) + 1);
    }

    public void CalculateArenaSpawnNumber()
    {
        SpawnNumberMaxTotalRemaining = Mathf.FloorToInt(SpawnNumberMaxTotalBase * (((GameController.Instance.GetCurrentDifficultyFactor() - 1) * DifficultyScaleSpawnNumber) + 1));
        ArenaWinThreshold = Mathf.CeilToInt(SpawnNumberMaxTotalRemaining * ArenaKillPercentageToWin);
    }


    // ================================= EFFECT: HEAL ALL PLAYERS ================================

    public void SetGlobalHealFactor(float NewFactor)
    {
        GlobalHealFactor = NewFactor;
    }

    public void EffectHealOnEnemyDeath(float HealPerc)
    {
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].gameObject.layer == CharacterPlayer.CharacterLayerID) // This checks if the Player is still alive.
            {
                Players[i].Heal(Players[i].GetHealthPercentageAbsoluteValue(HealPerc * GlobalHealFactor));
            }
        }
    }

    // ================================/ EFFECT: HEAL ALL PLAYERS /===============================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(spawnAreaMarker, SpawnAreaRadius);
    }
}
