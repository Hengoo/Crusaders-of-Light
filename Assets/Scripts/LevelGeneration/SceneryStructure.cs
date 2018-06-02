using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;

public class SceneryStructure
{
    public List<AreaSettings> Areas;

    public SceneryStructure(StoryStructure storyStructure, TerrainStructure terrainStructure)
    {
        // Assign paths to area settings
        CreateMainAndSidePathAreas(terrainStructure);

        // TODO: fill special areas - build structures
        // TODO: fill boss area - place logic elements
    }

    //---------------------------------------------------------------
    //
    //              CREATOR FUNCTIONS
    //
    //---------------------------------------------------------------

    // Create path areas
    private void CreateMainAndSidePathAreas(TerrainStructure terrainStructure)
    {
        // Main Paths
        var availableSegments = terrainStructure.AreaSegmentGraph.FindNodesWithData(new AreaSegment(AreaSegment.EAreaSegmentType.MainPath)).ToList();
        var availableSettings = terrainStructure.BiomeSettings.Path.ToList();

        while (availableSegments.Count > 0)
        {
            
        }
    }
}