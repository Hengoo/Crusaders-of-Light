using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public int PowerLevelCostPerLevelUp = 3;

    public int PowerlevelAllowance = 0;
    public int LevelMin = 0;    // Minimum Level of Weapons
    public int LevelMax = 0;    // Maximum Level of Weapons

    public Vector3[] Positions = new Vector3[0];
    public Vector3[] Rotations = new Vector3[0]; // Treated as Euler Angles

    public SpawnerSet[] SpawnSets = new SpawnerSet[0];
    public SpawnerSet[] RelevantSpawnSets = new SpawnerSet[0];

    public List<SpawnerOrder> OrdersForce = new List<SpawnerOrder>();
    public SpawnerOrder[] OrdersForbid = new SpawnerOrder[0];

    public TagBiome[] TagsBiome = new TagBiome[0];

    public void Initialize(int PowerLevel, int ItemLevelMin, int ItemLevelMax, Vector3[] SpawnPositions, Vector3[] SpawnRotation, TagBiome[] BiomeTags, SpawnerOrder[] SpawnOrders)
    {
        PowerlevelAllowance = PowerLevel;
        LevelMin = ItemLevelMin;
        LevelMax = ItemLevelMax;
        Positions = SpawnPositions;
        Rotations = SpawnRotation;
        TagsBiome = BiomeTags;

        List<SpawnerSet> AllowedSpawnSets = new List<SpawnerSet>();

        for (int i = 0; i < SpawnSets.Length; i++)
        {
            if (SpawnSets[i].CheckIfSpawnerSetAllowed(TagsBiome))
            {
                AllowedSpawnSets.Add(SpawnSets[i]);
            }
        }

        RelevantSpawnSets = AllowedSpawnSets.ToArray();

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


    public void SpawnEnemies()
    {
        if (RelevantSpawnSets.Length <= 0 || Positions.Length <= 0 || PowerlevelAllowance <= 0)
        {
            return;
        }

        int RemainingPowerLevel = PowerlevelAllowance;

        List<SpawnerEnemyObject.SpawnEnemy> GeneratedEnemies = new List<SpawnerEnemyObject.SpawnEnemy>();

        SpawnerSet CurrentSpawnerSet = null;
        SpawnerEnemyObject.SpawnEnemy CurrentEnemy = new SpawnerEnemyObject.SpawnEnemy();

        for (int i = 0; i < Positions.Length; i++)
        {
            CurrentSpawnerSet = RollForSpawnerSet();
            CurrentEnemy = CurrentSpawnerSet.GenerateEnemy(this);

            if (CurrentEnemy.PowerLevel <= RemainingPowerLevel) // TODO : Calc Powerlevel
            {
                GeneratedEnemies.Add(CurrentEnemy);
            }
            else
            {
                break;
            }
        }

        int RemainingLevelUps = Mathf.FloorToInt(RemainingPowerLevel / PowerLevelCostPerLevelUp);
        int CurrentEnemyLevelingUp = 0;

        while (RemainingLevelUps > 0)
        {
            // TODO : Distribute Level Ups
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
}
