using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner_Set_Master_List", menuName = "Spawner/SpawnerSetMasterList", order = 10)]
public class SpawnerSetMasterList : ScriptableObject {

    [Header("Master List Spawner Sets:")]
    public SpawnerSet[] SpawnSets = new SpawnerSet[0];

    public SpawnerSet[] GenerateAllowedSpawnSets(TagBiome[] BiomeTags)
    {
        List<SpawnerSet> AllowedSpawnSets = new List<SpawnerSet>();

        for (int i = 0; i < SpawnSets.Length; i++)
        {
            if (SpawnSets[i].CheckIfSpawnerSetAllowed(BiomeTags))
            {
                AllowedSpawnSets.Add(SpawnSets[i]);
            }
        }

        return AllowedSpawnSets.ToArray();
    }
}
