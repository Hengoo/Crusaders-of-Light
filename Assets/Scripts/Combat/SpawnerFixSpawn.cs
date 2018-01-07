using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawner_Enemy_Fix", menuName = "Spawner/SpawnerEnemyFix", order = 3)]
public class SpawnerFixSpawn : ScriptableObject {

    public SpawnerEnemyObject.SpawnEnemy SpawnEnemy;

    public SpawnerEnemyObject.SpawnEnemy GetSpawnEnemy()
    {
        return SpawnEnemy;
    }
}
