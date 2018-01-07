using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelController : Singleton<LevelController>
{
    public List<AreaBase> AreaPuzzlePool; //Pool of quests for an entire area available from start
    public List<AreaBase> QuestBossPool; //Pool of quests for the last area, aka Boss Area

    public QuestController QuestController;

    public void EndGame()
    {
        //TODO: implement
    }
}
