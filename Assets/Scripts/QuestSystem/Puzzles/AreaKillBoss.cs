using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class describes a "kill boss" area:
 * 
 * Quests:
 * -Find boss
 * -Kill boss 
 * 
 */

[CreateAssetMenu(fileName = "Area_KillBoss", menuName = "Areas/Kill boss")]
public class AreaKillBoss : AreaBase
{
    public GameObject BossPrefab1;
    public GameObject BossPrefab2;

    public override QuestBase[] GenerateQuests(SceneryStructure sceneryStructure, int assignedArea)
    {
        var result = new List<QuestBase>();

        //Boss spawn position
        var spawn = new GameObject("Area Boss Spawn Point");
        var spawnPosition = sceneryStructure.TerrainStructure.EndBiomeNode.Key;
        spawn.transform.position = new Vector3(spawnPosition.x, 0, spawnPosition.y);

        //Add objects to sceneryStructure for height adjustment
        sceneryStructure.AddSceneryQuestObject(spawn);

        //Find the boss quest
        var findBoss = new QuestReachPlace(spawn, 30, "The Evil", "Find the ultimate evil in this land");
        result.Add(findBoss);

        //Kill the boss quest
        var killBoss = new QuestKillEnemy(spawn.transform, new[] { BossPrefab1, BossPrefab2 }, "The Evil", "Cleanse the land from the evil!");
        result.Add(killBoss);

        return result.ToArray();
    }
}
