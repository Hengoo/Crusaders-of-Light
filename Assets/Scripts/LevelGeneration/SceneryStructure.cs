using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;

public class SceneryStructure
{
    public readonly List<AreaSettings> Areas = new List<AreaSettings>();

    private GrammarGraph<AreaSegment> _graph;

    public SceneryStructure(StoryStructure storyStructure, TerrainStructure terrainStructure)
    {
        _graph = new GrammarGraph<AreaSegment>(terrainStructure.AreaSegmentGraph);

        // Assign paths to area settings
        CreatePathAreas(terrainStructure);

        // TODO: fill special areas - build structures
        // TODO: fill boss area - place logic elements
    }

    //---------------------------------------------------------------
    //
    //              CREATOR FUNCTIONS
    //
    //---------------------------------------------------------------

    // Create path areas
    private void CreatePathAreas(TerrainStructure terrainStructure)
    {
        List<int> availableSegments = _graph.FindNodesWithData(new AreaSegment(AreaSegment.EAreaSegmentType.MainPath)).ToList();
        availableSegments = availableSegments.Union(_graph.FindNodesWithData(new AreaSegment(AreaSegment.EAreaSegmentType.SidePath))).ToList();
        availableSegments = availableSegments.Union(_graph.FindNodesWithData(new AreaSegment(AreaSegment.EAreaSegmentType.Start))).ToList();
        List<AreaSettingsFactory> availableSettings = terrainStructure.BiomeSettings.PathAreas.ToList();

        while (availableSegments.Count > 0 && availableSettings.Count > 0)
        {
            AreaSettingsFactory settingsFactory = availableSettings[Random.Range(0, availableSettings.Count)];
            Dictionary<int,int> matches = _graph.MatchPattern(settingsFactory.GetPatternGraph());

            if (matches == null)
            {
                availableSettings.Remove(settingsFactory);
                continue;
            }
            
            foreach (var match in matches)
            {
                availableSegments.Remove(match.Value);
                _graph.RemoveNode(match.Value);
            }

            Graph<AreaData> areaDataGraph = terrainStructure.GetAreaDataGraph(matches.Values);
            List<Vector2[]> clearPolygons = terrainStructure.GetPathPolygons(matches.Values);
            Vector2[] borderPolygon = terrainStructure.GetAreaSegmentsBorderPolygon(matches.Values);

            Areas.AddRange(settingsFactory.ProduceAreaSettings(areaDataGraph, clearPolygons, borderPolygon));
        }
    }
}