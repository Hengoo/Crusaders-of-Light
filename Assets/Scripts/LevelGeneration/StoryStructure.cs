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
        public readonly Graph<AreaSegment> Pattern = new Graph<AreaSegment>();
        public readonly Graph<AreaSegment> Replace = new Graph<AreaSegment>();
        public readonly Dictionary<int, int> Correspondences = new Dictionary<int, int>();

        public int AddNode(AreaSegment segment, AreaSegment correspondence)
        {
            int pat = Pattern.AddNode(segment);
            int rep = Replace.AddNode(correspondence);
            Correspondences.Add(pat, rep);

            return pat;
        }

        public void AddEdge(int l, int r, int newWeight)
        {
            Pattern.AddEdge(l, r, 0);
            Replace.AddEdge(Correspondences[l], Correspondences[r], newWeight);
        }

    }

    public int DifficultyLevel { get; private set; }
    public int LootAmount { get; private set; }
    public int MainPathLength { get; private set; }
    public AreaBase BossAreaConfiguration { get; private set; }
    public CharacterEnemy[] EnemySet { get; private set; }

    public readonly Queue<AreaSegmentRewrite> Rewrites;
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
        int t0, t1, t2;

        // Set start and end
        AreaSegmentRewrite startAndBoss = new AreaSegmentRewrite();
        t0 = startAndBoss.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Start));
        t1 = startAndBoss.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        startAndBoss.AddEdge(t0, t1, 1);

        // Main Path
        AreaSegmentRewrite mainPath = new AreaSegmentRewrite();
        t0 = mainPath.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Boss), new AreaSegment(AreaSegment.EAreaSegmentType.MainPath));
        t1 = mainPath.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        mainPath.AddEdge(t0, t1, 1);

        AreaSegmentRewrite extendPath = new AreaSegmentRewrite();
        t0 = extendPath.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.MainPath), new AreaSegment(AreaSegment.EAreaSegmentType.MainPath));
        t1 = extendPath.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.MainPath), new AreaSegment(AreaSegment.EAreaSegmentType.MainPath));
        t2 = extendPath.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.MainPath));
        extendPath.AddEdge(t0, t1, 0);
        extendPath.AddEdge(t0, t2, 1);
        extendPath.AddEdge(t2, t1, 1);


        // Enqueue all segment rewrites
        Rewrites.Enqueue(startAndBoss);
        for(int i = 0; i < MainPathLength; ++i)
            Rewrites.Enqueue(mainPath);
        Rewrites.Enqueue(extendPath);

    }
}