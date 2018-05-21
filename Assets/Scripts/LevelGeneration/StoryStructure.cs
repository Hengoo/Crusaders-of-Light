using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoryStructure
{
    public class AreaSegmentRewrite
    {
        public Graph<AreaSegment> Pattern = new Graph<AreaSegment>();
        public Graph<AreaSegment> Replace = new Graph<AreaSegment>();
    }

    public int DifficultyLevel { get; private set; }
    public int LootAmount { get; private set; }
    public int MainPathLength { get; private set; }
    public AreaBase BossAreaConfiguration { get; private set; }
    public CharacterEnemy[] EnemySet { get; private set; }

    public Queue<AreaSegmentRewrite> Rewrites;
    //public Element Reward;

    public StoryStructure(int difficultyLevel, int lootAmount, int mainPathLength, AreaBase bossAreaConfiguration, CharacterEnemy[] enemySet)
    {
        DifficultyLevel = difficultyLevel;
        LootAmount = lootAmount;
        MainPathLength = mainPathLength;
        BossAreaConfiguration = bossAreaConfiguration;
        EnemySet = enemySet;

        Rewrites = new Queue<AreaSegmentRewrite>();

        // Fill patterns to rework 
        CreateRewrites();
    }

    private void CreateRewrites()
    {
        // Set start and end
        AreaSegmentRewrite startBoss = new AreaSegmentRewrite();
        int empty0 = startBoss.Pattern.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty));
        int empty1 = startBoss.Pattern.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty));
        startBoss.Pattern.AddEdge(empty0, empty1, 1);
        int start = startBoss.Replace.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Start));
        int end = startBoss.Replace.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        startBoss.Replace.AddEdge(start, end, 1);

        // Main Path - TODO
        AreaSegmentRewrite createMainPath = new AreaSegmentRewrite();
        createMainPath.Pattern.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Start));

        // Enqueue all segment rewrites
        Rewrites.Enqueue(startBoss);
    }
}