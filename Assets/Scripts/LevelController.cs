using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelController : Singleton<LevelController>
{
    public List<AreaBase> AreaPuzzlePool; //Pool of quests for an entire area available from start
    public List<AreaBase> QuestBossPool; //Pool of quests for the last area, aka Boss Area

    public QuestController QuestController;

    public CharacterPlayer[] Players;

    public SceneryStructure SceneryStructure;
    public Terrain Terrain;
    
    public void InitializeLevel()
    {
        QuestController.ClearQuests();
    }

    public void StartGame(SceneryStructure sceneryStructure, Terrain terrain)
    {
        SceneryStructure = sceneryStructure;
        Terrain = terrain;
        
        var terrainStructure = SceneryStructure.TerrainStructure;
        var startPosition2D = terrainStructure.BiomeGraph.GetNodeData(terrainStructure.StartBiomeNode.Value).Center;
        for (var i = 0; i < Players.Length; i++)
        {
            Vector3 spawnPosition = new Vector3(startPosition2D.x, 0, startPosition2D.y);
            switch (i)
            {
                case 0:
                    spawnPosition += new Vector3(3,0,0);
                    break;
                case 1:
                    spawnPosition += new Vector3(0, 0, 3);
                    break;
                case 2:
                    spawnPosition += new Vector3(-3, 0, 0);
                    break;
                case 3:
                    spawnPosition += new Vector3(0, 0, -3);
                    break;
            }
            spawnPosition = new Vector3(spawnPosition.x, Terrain.SampleHeight(spawnPosition) + 0.05f, spawnPosition.z);
            Players[i].transform.position = spawnPosition;
        }
    }

    public void EndGame()
    {
        //TODO: implement
    }
}
