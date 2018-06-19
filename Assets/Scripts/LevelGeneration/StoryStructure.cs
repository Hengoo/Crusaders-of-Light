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
    public CharacterEnemy[] EnemySet { get; private set; }

    public readonly Queue<AreaSegmentRewrite> Rewrites;
    //public Element Reward;

    public StoryStructure(int difficultyLevel, int lootAmount, int mainPathLength, int sidePathCount, int sidePathLength, CharacterEnemy[] enemySet)
    {
        DifficultyLevel = difficultyLevel;
        LootAmount = lootAmount;
        MainPathLength = mainPathLength;
        SidePathCount = sidePathCount;
        SidePathLength = sidePathLength;
        EnemySet = enemySet;

        Rewrites = new Queue<AreaSegmentRewrite>();

        // Fill patterns to rework 
        CreateRewrites();
    }

    private void CreateRewrites()
    {
        int sidePathMod = SidePathCount > 0 ? (MainPathLength + 1) / SidePathCount : 1;

        // Set start and end
        AreaSegmentRewrite areaSegmentRewrite = new AreaSegmentRewrite();
        int start = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Start));

        // Boss arena 3 node cluster
        int boss0 = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        int boss1 = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss)); // Boss adjacent node
        int boss2 = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.Boss)); // Boss adjacent node
        areaSegmentRewrite.AddEdge(boss0, boss2, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);
        areaSegmentRewrite.AddEdge(boss2, boss1, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);
        areaSegmentRewrite.AddEdge(boss1, boss0, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);

        // Main Path
        var mainPathNodes = new List<int>();
        var mainPathPrev = start;
        for (int i = 0; i < MainPathLength; i++)
        {
            var mainPathCur = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty), new AreaSegment(AreaSegment.EAreaSegmentType.MainPath));
            areaSegmentRewrite.AddEdge(mainPathPrev, mainPathCur, (int)AreaSegment.EAreaSegmentEdgeType.MainPath);
            mainPathNodes.Add(mainPathCur);
            mainPathPrev = mainPathCur;
        }
        areaSegmentRewrite.AddEdge(mainPathPrev, boss0, (int)AreaSegment.EAreaSegmentEdgeType.MainPath); // MainPath-Boss edge

        // Side Paths
        int count = 0;
        while (mainPathNodes.Count > 0 && count < SidePathCount)
        {
            // Select random main path node
            var mainPath = mainPathNodes[Random.Range(0, mainPathNodes.Count)];
            mainPathNodes.RemoveAll(
                e => e == mainPath || areaSegmentRewrite.Pattern.GetNeighbours(mainPath).Contains(e));

            // Create side path off the selected Main Path Node
            var sidePathPrev = mainPath;
            for (int j = 0; j < SidePathLength; j++)
            {
                var sidePathCur = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty),
                    new AreaSegment(AreaSegment.EAreaSegmentType.SidePath));
                areaSegmentRewrite.AddEdge(sidePathPrev, sidePathCur, (int)AreaSegment.EAreaSegmentEdgeType.SidePath);
                sidePathPrev = sidePathCur;
            }

            // Add special area at the end
            int specialArea = areaSegmentRewrite.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Empty),
                new AreaSegment(AreaSegment.EAreaSegmentType.Special));
            areaSegmentRewrite.AddEdge(sidePathPrev, specialArea, (int)AreaSegment.EAreaSegmentEdgeType.SidePath);

            count++;
        }

        Rewrites.Enqueue(areaSegmentRewrite);
    }
}