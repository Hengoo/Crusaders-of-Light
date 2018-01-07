using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    [Header("Spawner Power Level and Skill Levels:")]
    public int PowerLevelCostPerLevelUp = 3;

    [Header("Spawner Spawn Sets Master List:")]
    public SpawnerSetMasterList SpawnSets;

    [Header("Spawn Interaction:")]
    public string Tag = "Attention";
    private bool AlreadySpawned = false;

    [Header("Spawner Internal (Shown for Debugging - Do not set in Editor):")]
    public int PowerlevelAllowance = 0;
    public int LevelMin = 0;    // Minimum Level of Weapons
    public int LevelMax = 0;    // Maximum Level of Weapons

    public Vector3[] Positions = new Vector3[0];
    public Vector3[] Rotations = new Vector3[0]; // Treated as Euler Angles

    public SpawnerSet[] RelevantSpawnSets = new SpawnerSet[0];

    public List<SpawnerOrder> OrdersForce = new List<SpawnerOrder>();
    public SpawnerOrder[] OrdersForbid = new SpawnerOrder[0];

    public SpawnerFixSpawn[] FixSpawns = new SpawnerFixSpawn[0];
    public int FixSpawnsCounter = 0;

    public TagBiome[] TagsBiome = new TagBiome[0];

    public SpawnerEnemyObject.SpawnEnemy[] GeneratedEnemies = new SpawnerEnemyObject.SpawnEnemy[0];
    public List<CharacterEnemy> SpawnedEnemies = new List<CharacterEnemy>();

    public void Initialize(bool GenerateEnemiesNow, int PowerLevel, int ItemLevelMin, int ItemLevelMax, Vector3[] SpawnPositions, Vector3[] SpawnRotations, TagBiome[] BiomeTags)
    {
        PowerlevelAllowance = PowerLevel;
        LevelMin = ItemLevelMin;
        LevelMax = ItemLevelMax;
        Positions = SpawnPositions;
        Rotations = SpawnRotations;
        TagsBiome = BiomeTags;

        RelevantSpawnSets = SpawnSets.GenerateAllowedSpawnSets(TagsBiome);

        if (GenerateEnemiesNow)
        {
            GenerateEnemies();
        }
    }

    public void Initialize(bool GenerateEnemiesNow, int PowerLevel, int ItemLevelMin, int ItemLevelMax, Vector3[] SpawnPositions, Vector3[] SpawnRotations, TagBiome[] BiomeTags, SpawnerOrder[] SpawnOrders)
    {
        InitializeSpawnOrders(SpawnOrders);
        Initialize(GenerateEnemiesNow, PowerLevel, ItemLevelMin, ItemLevelMax, SpawnPositions, SpawnRotations, BiomeTags);
    }

    private void InitializeSpawnOrders(SpawnerOrder[] SpawnOrders)
    {
        List<SpawnerOrder> OrdersForbidList = new List<SpawnerOrder>();

        for (int i = 0; i < SpawnOrders.Length; i++)
        {
            if (SpawnOrders[i].GetMode() == SpawnerOrder.Mode.FORCE_ONE)
            {
                OrdersForce.Add(SpawnOrders[i]);
            }
            else
            {
                OrdersForbidList.Add(SpawnOrders[i]);
            }
        }

        OrdersForbid = OrdersForbidList.ToArray();
    }

    public void Initialize(bool GenerateEnemiesNow, int PowerLevel, int ItemLevelMin, int ItemLevelMax, Vector3[] SpawnPositions, Vector3[] SpawnRotations, TagBiome[] BiomeTags, SpawnerFixSpawn[] FixedSpawns)
    {
        InitializeFixSpawns(FixedSpawns);
        Initialize(GenerateEnemiesNow, PowerLevel, ItemLevelMin, ItemLevelMax, SpawnPositions, SpawnRotations, BiomeTags);
    }

    private void InitializeFixSpawns(SpawnerFixSpawn[] FixedSpawns)
    {
        FixSpawns = FixedSpawns;
        FixSpawnsCounter = 0;
    }

    public void Initialize(bool GenerateEnemiesNow, int PowerLevel, int ItemLevelMin, int ItemLevelMax, Vector3[] SpawnPositions, Vector3[] SpawnRotations, TagBiome[] BiomeTags, SpawnerOrder[] SpawnOrders, SpawnerFixSpawn[] FixedSpawns)
    {
        InitializeSpawnOrders(SpawnOrders);
        InitializeFixSpawns(FixedSpawns);
        Initialize(GenerateEnemiesNow, PowerLevel, ItemLevelMin, ItemLevelMax, SpawnPositions, SpawnRotations, BiomeTags);
    }


    public void GenerateEnemies() // Generates the Enemies. Note: Does not automatically spawn them. For this, call SpawnEnemies().
    {
        if (RelevantSpawnSets.Length <= 0 || Positions.Length <= 0 || PowerlevelAllowance <= 0)
        {
            return;
        }

        int RemainingPowerLevel = PowerlevelAllowance;

        List<SpawnerEnemyObject.SpawnEnemy> CurrentlyGeneratedEnemies = new List<SpawnerEnemyObject.SpawnEnemy>();

        SpawnerSet CurrentSpawnerSet = null;
        SpawnerEnemyObject.SpawnEnemy CurrentEnemy = new SpawnerEnemyObject.SpawnEnemy();

        for (int i = 0; i < Positions.Length; i++)
        {
            if (FixSpawnsCounter < FixSpawns.Length)
            {
                CurrentEnemy = FixSpawns[FixSpawnsCounter].GetSpawnEnemy();
                FixSpawnsCounter++;
            }
            else
            {
                CurrentSpawnerSet = RollForSpawnerSet();
                CurrentEnemy = CurrentSpawnerSet.GenerateEnemy(this);
            }

            if (CurrentEnemy.PowerLevel <= RemainingPowerLevel)
            {
                //Debug.Log("GENERATED ENEMEY WITH POWER LEVEL : " + CurrentEnemy.PowerLevel);
                RemainingPowerLevel -= CurrentEnemy.PowerLevel;
                //Debug.Log("REMAINING POWER LEVEL: " + RemainingPowerLevel);
                CurrentlyGeneratedEnemies.Add(CurrentEnemy);          
            }
            // If not enough Powerlevel remaining: Chance to still spawn the enemy: Weighted Chance between Remaining PowerLevel (Spawn) and Enemy Cost (don't Spawn).
            else if (Random.Range(0, CurrentEnemy.PowerLevel + RemainingPowerLevel) < RemainingPowerLevel) 
            {
                CurrentlyGeneratedEnemies.Add(CurrentEnemy);
                //Debug.Log("GENERATED ENEMEY WITH POWER LEVEL : " + CurrentEnemy.PowerLevel + " BY CHANCE!");
                break;
            }
            else
            {
                //Debug.Log("DID NOT GENERATED ENEMY DUE TO NOT ENOUGH POWER LEVEL!");
                break;
            }
        }

        GeneratedEnemies = CurrentlyGeneratedEnemies.ToArray();    
    }

    public void SpawnEnemies() // Spawns the previously generated Enemies. If no Enemies have been generated yet, it generates them.
    {
        if (GeneratedEnemies.Length == 0)
        {
            GenerateEnemies();
        }

        CharacterEnemy CurrentlySpawningEnemy = null;

        for (int i = 0; i < GeneratedEnemies.Length; i++)
        {
            CurrentlySpawningEnemy = Instantiate(GeneratedEnemies[i].CharacterBase, Positions[i], Quaternion.Euler(Rotations[i]));
            CurrentlySpawningEnemy.InitializeEnemy(this, GeneratedEnemies[i].Weapons, GeneratedEnemies[i].WeaponLevels);
            SpawnedEnemies.Add(CurrentlySpawningEnemy);
        }
    }

    private SpawnerSet RollForSpawnerSet()
    {
        int RolledNumber = Random.Range(0, RelevantSpawnSets.Length - 1);

        return RelevantSpawnSets[RolledNumber];
    }

    public List<SpawnerOrder> GetSpawnOrdersForce()
    {
        return OrdersForce;
    }

    public void SpawnOrderForceFulfilled(SpawnerOrder FulfilledOrder)
    {
        OrdersForce.Remove(FulfilledOrder);
    }

    public SpawnerOrder[] GetSpawnOrderForbid()
    {
        return OrdersForbid;
    }

    public int GetMinLevel()
    {
        return LevelMin;
    }

    public int GetMaxLevel()
    {
        return LevelMax;
    }

    public int GetPowerLevelCostPerLevelUp()
    {
        return PowerLevelCostPerLevelUp;
    }

    public void SpawnedCharacterDied(CharacterEnemy DeadCharacter)
    {
        SpawnedEnemies.Remove(DeadCharacter);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (AlreadySpawned)
        {
            return;
        }

        if (other.tag == Tag)
        {
            CharacterAttention OtherAttention = other.gameObject.GetComponent<CharacterAttention>();

            if (OtherAttention.GetOwner().GetAlignment() == Character.TeamAlignment.PLAYERS)
            {
                SpawnEnemies();
                AlreadySpawned = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.2f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 2);

        Gizmos.color = new Color(1f, 0.2f, 1f, 0.5f);
        Gizmos.DrawSphere(transform.position, 2);
    }
}
