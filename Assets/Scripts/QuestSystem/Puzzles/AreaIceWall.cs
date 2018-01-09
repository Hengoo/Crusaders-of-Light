using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
/*
 * This class creates the Ice Wall area
 * 
 * Quests:
 *  -Find Ice Wall
 *  -Find Fire Mage camp
 *  -Kill Fire Mage
 *  -Pickup Fire Mage Staff
 *  -Destroy the Ice Wall
 *  
 */

[CreateAssetMenu(fileName = "Area_IceWall", menuName = "Areas/Ice wall")]
public class AreaIceWall : AreaBase
{

    public GameObject IceWallPrefab;
    public GameObject FireMage; //Must have enemy character script attached to it

    [HideInInspector] private readonly List<QuestBase> _questSteps = new List<QuestBase>();

    //Generate the quests to be given to the quest controller
    public override QuestBase[] GenerateQuests(SceneryStructure sceneryStructure, int assignedArea)
    {
        var worldStructure = sceneryStructure.WorldStructure;
        var terrainStructure = sceneryStructure.TerrainStructure;

        //Get a random position for the fire mage TODO: improve this selection if needed
        int node;
        int desiredNeighbors = 1;
        var availableBiomes = worldStructure.AreaBiomes[assignedArea].ToList();
        var startNode = terrainStructure.StartBiomeNode.Value;
        bool found = false;
        do
        {
            var temp = new List<int>(availableBiomes);
            do
            {
                node = temp[Random.Range(0, temp.Count)];
                temp.Remove(node);
                var neighborhood = worldStructure.NavigationGraph.GetNeighbours(node);

                found = !(neighborhood.Length > desiredNeighbors ||
                          !neighborhood.All(a => availableBiomes.Contains(a)) ||
                          neighborhood.Contains(terrainStructure.StartBiomeNode.Value) ||
                          node == startNode ||
                          worldStructure.AreaCrossingNavigationEdges.Any(a => a.x == node || a.y == node));
            } while (!found && temp.Count > 0);
            desiredNeighbors++;

        } while (desiredNeighbors <= 4 && !found);
        

        var spawnPosition2D = terrainStructure.BiomeGraph.GetNodeData(node).Center;
        var fireMageSpawn = new GameObject("Fire Mage Spawn Point");

        fireMageSpawn.transform.position = new Vector3(spawnPosition2D.x, 0, spawnPosition2D.y);

        //Create ice wall in the game world
        var iceWallCrossingLine = worldStructure.AreaCrossingBorders[assignedArea];
        var iceWallPosition2D = (iceWallCrossingLine[0] + iceWallCrossingLine[1]) / 2;
        var iceWallOrientationLine = iceWallCrossingLine[1] - iceWallCrossingLine[0];
        var iceWall = Instantiate(IceWallPrefab);
        iceWall.transform.position = new Vector3(iceWallPosition2D.x, 0, iceWallPosition2D.y);
        iceWall.transform.localScale = new Vector3(iceWallOrientationLine.magnitude * 1.2f / 4f, terrainStructure.BiomeGlobalConfiguration.MapHeight /3f, 15 / 1.5f);
        iceWall.transform.rotation = Quaternion.LookRotation(iceWall.transform.position + Vector3.Cross(new Vector3(iceWallOrientationLine.x, 0, iceWallOrientationLine.y), Vector3.up) * 10);

        //Add GameObjects to the scenery objects list (for height adjustment)
        sceneryStructure.AddSceneryQuestObject(fireMageSpawn);
        sceneryStructure.AddSceneryQuestObject(iceWall);
        
        //Find ice wall
        _questSteps.Add(new QuestReachPlace(iceWall, .3f, "The Wall", "Explore the area and find the ice wall location"));

        //Find fire mage camp
        _questSteps.Add(new QuestReachPlace(fireMageSpawn, 30, "The Wall", "Find the fire wizard"));

        //Kill fire mage
        _questSteps.Add(new QuestKillEnemy(fireMageSpawn.transform, FireMage, "The Wall", "Kill the fire wizard"));

        //Pickup fire mage staff
        _questSteps.Add(new QuestPickupItem(FireMage.GetComponent<Character>().StartingWeapons[1], "The Wall", "Pickup the fire mage staff"));

        //Destroy the ice wall
        _questSteps.Add(new QuestDestroyBuilding(iceWall.GetComponent<CharacterEnemy>(), "The Wall", "Destroy the wall with the fire staff"));

        return _questSteps.ToArray();
    }
}
