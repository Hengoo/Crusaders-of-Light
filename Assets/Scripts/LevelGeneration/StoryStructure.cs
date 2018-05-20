using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoryStructure
{
    public int DifficultyLevel { get; private set; }
    public int LootAmount { get; private set; }
    public int MainPathLength { get; private set; }
    public AreaBase BossArea { get; private set; }
    public CharacterEnemy[] EnemySet { get; private set; }
    //public Element Reward;

    public StoryStructure(int difficultyLevel, int lootAmount, int mainPathLength, AreaBase bossArea, CharacterEnemy[] enemySet)
    {
        DifficultyLevel = difficultyLevel;
        LootAmount = lootAmount;
        MainPathLength = mainPathLength;
        BossArea = bossArea;
        EnemySet = enemySet;
    }    
}