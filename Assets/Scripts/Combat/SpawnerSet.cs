using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner_Set", menuName = "Spawner/SpawnerSet", order = 1)]
public class SpawnerSet : ScriptableObject {

    [System.Serializable]
    public struct SpawnEnemyObject
    {
        public SpawnerEnemyObject Enemy;
        public int Weight;
    }

    [Header("Spawner Set Limitations:")]
    public TagBiome[] RequiredBiomeTags = new TagBiome[0];

    [Header("Spawner Set Characters:")]
    public SpawnEnemyObject[] SpawnEnemies = new SpawnEnemyObject[0];


    public SpawnerEnemyObject.SpawnEnemy GenerateEnemy(Spawner MainSpawner)
    {
        int TotalWeight = 0;

        for (int i = 0; i < SpawnEnemies.Length; i++)
        {
            TotalWeight += SpawnEnemies[i].Weight;
        }

        int RolledNumber = Random.Range(0, TotalWeight);
        int WeightCounter = 0;

        for (int i = 0; i < SpawnEnemies.Length; i++)
        {
            WeightCounter += SpawnEnemies[i].Weight;

            if (RolledNumber < WeightCounter)
            {
                return SpawnEnemies[i].Enemy.GenerateEnemy(MainSpawner);
            }
        }

        return new SpawnerEnemyObject.SpawnEnemy(); // Should not be reachable!
    }


    public bool CheckIfSpawnerSetAllowed(TagBiome[] TagBiomeList)
    {
        bool FoundTag = false;

        for (int i = 0; i < RequiredBiomeTags.Length; i++)
        {
            FoundTag = false;

            for (int j = 0; j < TagBiomeList.Length; j++)
            {
                if (RequiredBiomeTags[i] == TagBiomeList[j])
                {
                    FoundTag = true;
                    break;
                }
            }

            if (!FoundTag)
            {
                return false;
            }
        }

        return true;
    }

}
