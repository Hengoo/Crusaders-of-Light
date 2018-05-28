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
    public int SidePathCount { get; private set; }
    public int SidePathLength { get; private set; }
    public AreaBase BossAreaConfiguration { get; private set; }
    public CharacterEnemy[] EnemySet { get; private set; }

    public readonly Queue<AreaSegmentRewrite> Rewrites;
    //public Element Reward;

    public StoryStructure(int difficultyLevel, int lootAmount, int mainPathLength, int sidePathCount, int sidePathLength, AreaBase bossAreaConfiguration, CharacterEnemy[] enemySet)
    {
        DifficultyLevel = difficultyLevel;
        LootAmount = lootAmount;
        MainPathLength = mainPathLength;
        SidePathCount = sidePathCount;
        SidePathLength = sidePathLength;
        BossAreaConfiguration = bossAreaConfiguration;
        EnemySet = enemySet;

        Rewrites = new Queue<AreaSegmentRewrite>();

        // Fill patterns to rework 
        CreateRewrites();
    }

    private void CreateRewrites()
    {
        int start, bossEntry, t0, t1, t2, t3;
        int sidePathMod = MainPathLength / SidePathCount;

        // Set start and end
        AreaSegmentRewrite everything = new AreaSegmentRewrite();
        start = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Start)); // Start node

        // Boss arena 3 node cluster
        bossEntry = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss)); // Boss entrance node
        t0 = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss)); // Boss adjacent node
        t1 = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss)); // Boss adjacent node
        everything.AddEdge(bossEntry, t0, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);
        everything.AddEdge(t0, t1, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);
        everything.AddEdge(t1, bossEntry, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);

        // Main Path
        t0 = start;
        for (int i = 0; i < MainPathLength; i++)
        {
            t1 = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.MainPath)); // MainPath-MainPath node and edge
            everything.AddEdge(t0, t1, (int)AreaSegment.EAreaSegmentEdgeType.MainPath);

            // Side Paths
            if ((i + 1) % sidePathMod == 0)
            {
                int specialArea = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty),
                    new AreaSegment(AreaSegment.EAreaSegmentType.Special));
                t2 = t1;
                for (int j = 0; j < SidePathLength; j++)
                {
                    t3 = everything.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty),
                        new AreaSegment(AreaSegment.EAreaSegmentType.SidePath));
                    everything.AddEdge(t2, t3, (int)AreaSegment.EAreaSegmentEdgeType.SidePath);
                    t2 = t3;
                }
                everything.AddEdge(t2, specialArea, (int)AreaSegment.EAreaSegmentEdgeType.SidePath);
            }

            t0 = t1;
        }
        everything.AddEdge(t0, bossEntry, (int)AreaSegment.EAreaSegmentEdgeType.MainPath); // MainPath-Boss edge

        Rewrites.Enqueue(everything);
    }
}