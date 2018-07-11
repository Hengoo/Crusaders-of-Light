using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "skill_effect_spawn_swarmling", menuName = "Combat/SkillEffects/SpawnSwarmling", order = 40)]
public class SkillEffectSpawnSwarmlings : SkillEffect {

    public EnemySwarm SwarmlingPrefab;
    public int NumberToSpawn = 0;
    public float SpawnRadius = 10;

    public float GlobalHealMod = 1;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        LevelController.Instance.GetSwarmSpawner().SpawnEnemyBatch(NumberToSpawn, SwarmlingPrefab, SourceItemSkill.transform.position, SpawnRadius);
        LevelController.Instance.GetSwarmSpawner().SetGlobalHealFactor(GlobalHealMod);
    }
}
