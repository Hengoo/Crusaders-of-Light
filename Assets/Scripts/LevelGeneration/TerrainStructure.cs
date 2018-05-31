using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using csDelaunay;


public class TerrainStructure
{

    public Voronoi VoronoiDiagram { get; private set; }
    public GrammarGraph<AreaSegment> AreaSegmentGraph { get; private set; }
    public List<Area> Areas { get; private set; }
    public KeyValuePair<Vector2f, int> StartAreaSegment;

    public int TextureCount { get { return _splatIDMap.Count; } }

    public float MapSize { get; private set; }
    public int HeightMapResolution { get; private set; }
    public int Octaves { get; private set; }
    public int RoadSplatIndex { get; private set; }

    public readonly List<Vector2> OuterBorderPolygon;
    public readonly List<Vector2[]> RoadLines;
    public readonly List<Vector2[]> AreaBorders;

    private readonly Dictionary<int, Vector2> _areaSegmentCenterMap = new Dictionary<int, Vector2>(); //Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<Vector2f, int> _siteAreaMap = new Dictionary<Vector2f, int>(); //Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<SplatPrototypeSerializable, int> _splatIDMap = new Dictionary<SplatPrototypeSerializable, int>(); //Mapping of biome SplatPrototypes and terrain texture IDs

    private Texture2D _blankSpec;
    private Texture2D _blankBump;

    private readonly SplatPrototypeSerializable _roadSplatPrototype;
    private readonly int _voronoiSamples;

    private readonly BiomeSettings _biomeSettings;
    private readonly BiomeSettings _borderSettings;
    private readonly float _borderNoise;

    public TerrainStructure(StoryStructure storyStructure, GlobalSettings globalSettings, List<BiomeSettings> availableBiomes)
    {
        AreaSegmentGraph = new GrammarGraph<AreaSegment>();
        Areas = new List<Area>();
        OuterBorderPolygon = new List<Vector2>();
        AreaBorders = new List<Vector2[]>();
        RoadLines = new List<Vector2[]>();

        // Select a random biome out of the available ones for the current level
        _biomeSettings = availableBiomes[Random.Range(0, availableBiomes.Count)];

        MapSize = globalSettings.MapSize;
        HeightMapResolution = globalSettings.HeightMapResolution;
        Octaves = globalSettings.Octaves;

        _borderSettings = globalSettings.BorderBiome;
        _roadSplatPrototype = globalSettings.RoadSplatPrototype;
        _voronoiSamples = globalSettings.VoronoiSamples;
        _borderNoise = globalSettings.EdgeNoise;

        // Add splat prototypes to the shader
        AddShaderTextures();

        // Create base graph that later on is transformed with a set of rules and assigned areas to
        CreateBaseGraph(globalSettings);

        // Assign specific areas to each node of the base graph - Start point, Boss arena, paths...
        CreateAreaGraph(storyStructure);
    }

    /* Returns the center position for a given area segment ID */
    public Vector2 GetAreaSegmentCenter(int id)
    {
        Vector2 value;
        return _areaSegmentCenterMap.TryGetValue(id, out value) ? value : Vector2.zero;
    }

    /* Returns a sorted list of the textures */
    public IEnumerable<Texture> GetTerrainTextures()
    {
        var result = new SortedList<int, Texture>();

        foreach (var splatID in _splatIDMap)
        {
            result.Add(splatID.Value, splatID.Key.texture);
        }

        return result.Values;
    }

    /* Returns a sorted list of the splats */
    public SplatPrototype[] GetSplatPrototypes()
    {
        var result = new SortedList<int, SplatPrototype>();

        foreach (var splatID in _splatIDMap)
        {
            var splatPrototype = new SplatPrototype()
            {
                texture = splatID.Key.texture,
                normalMap = splatID.Key.normalMap,
                smoothness = splatID.Key.smoothness,
                metallic = splatID.Key.metallic,
                tileSize = splatID.Key.tileSize,
                tileOffset = splatID.Key.tileOffset
            };
            result.Add(splatID.Value, splatPrototype);
        }

        return result.Values.ToArray();
    }

    /* Get all biome borders */
    public IEnumerable<LineSegment> GetAllAreaSegmentBorders()
    {
        //TODO REIMPLEMENT
        var result = new List<LineSegment>();

        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftArea = AreaSegmentGraph.GetNodeData(_siteAreaMap[edge.LeftSite.Coord]);
            var rightArea = AreaSegmentGraph.GetNodeData(_siteAreaMap[edge.RightSite.Coord]);
            if (_siteAreaMap[edge.LeftSite.Coord] == _siteAreaMap[edge.RightSite.Coord])
                continue;

            var p0 = edge.ClippedEnds[LR.LEFT];
            var p1 = edge.ClippedEnds[LR.RIGHT];
            var segment = new LineSegment(p0, p1);
            result.Add(segment);
        }

        return result;
    }

    public IEnumerable<Vector2[]> GetPathLines()
    {
        List<Vector2[]> pathLines = new List<Vector2[]>();

        return pathLines;
    }

    /* Sample area height at a given position */
    public BiomeHeightParameters SampleHeight(Vector2 position)
    {
        Vector2 noisePosition = position + new Vector2(Random.Range(-_borderNoise, _borderNoise), Random.Range(-_borderNoise, _borderNoise));
        AreaSegment areaSegment = GetClosestAreaSegment(noisePosition);
        return areaSegment == null || areaSegment.Type == AreaSegment.EAreaSegmentType.Border ? _borderSettings.HeightParameters : _biomeSettings.HeightParameters;
    }

    /* Sample area texture at given position */
    public KeyValuePair<int, float> SampleTexture(Vector2 position)
    {
        Vector2 noisePosition = position + new Vector2(Random.Range(-_borderNoise, _borderNoise), Random.Range(-_borderNoise, _borderNoise));
        AreaSegment areaSegment = GetClosestAreaSegment(noisePosition);
        int splatID = _splatIDMap[areaSegment.Type == AreaSegment.EAreaSegmentType.Border ? _borderSettings.Splat : _biomeSettings.Splat];
        float splatValue = 1;
        return new KeyValuePair<int, float>(splatID, splatValue);
    }

    /* Return biome ID from a csDelaunay vector */
    public int GetNodeIDFromSite(Vector2f coord)
    {
        return _siteAreaMap[coord];
    }

    //---------------------------------------------------------------
    //
    //              HELPER FUNCTIONS
    //
    //---------------------------------------------------------------

    /* Assign textures to shader */
    private void AddShaderTextures()
    {
        // Add Splat textures to global shader variables
        _blankBump = GenerateBlankNormal();
        _blankSpec = GenerateBlankSpec();
        var count = 0;

        // Add main biome to the SplatPrototypes map
        _splatIDMap.Add(_biomeSettings.Splat, count);
        Shader.SetGlobalTexture("_BumpMap" + count, _biomeSettings.Splat.normalMap ? _biomeSettings.Splat.normalMap : _blankBump);
        Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
        Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / _biomeSettings.Splat.tileSize.x);
        count++;


        // Add border biome to the SplatPrototypes map
        if (!_splatIDMap.ContainsKey(_borderSettings.Splat))
        {
            _splatIDMap.Add(_borderSettings.Splat, count);

            Shader.SetGlobalTexture("_BumpMap" + count, _borderSettings.Splat.normalMap ? _borderSettings.Splat.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / _borderSettings.Splat.tileSize.x);
            count++;
        }

        // Add road to the SplatPrototypes map
        if (!_splatIDMap.ContainsKey(_roadSplatPrototype))
        {
            _splatIDMap.Add(_roadSplatPrototype, count);

            Shader.SetGlobalTexture("_BumpMap" + count, _roadSplatPrototype.normalMap ? _roadSplatPrototype.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / _roadSplatPrototype.tileSize.x);
            RoadSplatIndex = count;
        }
    }

    /* Create a graph containing all connected empty areas */
    private void CreateBaseGraph(GlobalSettings globalSettings)
    {
        // Create uniform random point distribution and Voronoi Diagram
        var centers = new List<Vector2f>();
        for (int i = 0; i < _voronoiSamples; i++)
        {
            var x = Random.Range(0f, MapSize);
            var y = Random.Range(0f, MapSize);
            centers.Add(new Vector2f(x, y));
        }
        VoronoiDiagram = new Voronoi(centers, new Rectf(0, 0, MapSize, MapSize));

        // Apply Lloyd Relaxation
        VoronoiDiagram.LloydRelaxation(globalSettings.LloydRelaxation);

        // Assign area segments to initial areas
        foreach (var site in VoronoiDiagram.SiteCoords())
        {
            bool isOnBorder = false;
            var segments = VoronoiDiagram.VoronoiBoundaryForSite(site);

            foreach (var segment in segments)
            {
                if (segment.p0.x <= VoronoiDiagram.PlotBounds.left || segment.p0.x >= VoronoiDiagram.PlotBounds.right
                    || segment.p0.y <= VoronoiDiagram.PlotBounds.top || segment.p0.y >= VoronoiDiagram.PlotBounds.bottom
                    || segment.p1.x <= VoronoiDiagram.PlotBounds.left || segment.p1.x >= VoronoiDiagram.PlotBounds.right
                    || segment.p1.y <= VoronoiDiagram.PlotBounds.top ||
                    segment.p1.y >= VoronoiDiagram.PlotBounds.bottom)
                {
                    isOnBorder = true;
                    break;
                }
            }

            // Assign areaSegment to site and corresponding area
            var areaSegment = new AreaSegment(isOnBorder ? AreaSegment.EAreaSegmentType.Border : AreaSegment.EAreaSegmentType.Empty);

            var nodeID = AreaSegmentGraph.AddNode(areaSegment);
            _siteAreaMap.Add(site, nodeID);
            _areaSegmentCenterMap.Add(nodeID, site.ToUnityVector2());
        }

        // Create navigation graph - for each area segment that is not a border, add reachable neighbors
        foreach (var id in _siteAreaMap)
        {
            var areaSegment = AreaSegmentGraph.GetNodeData(id.Value);
            if (areaSegment.Type == AreaSegment.EAreaSegmentType.Border) continue;

            Vector2 center = _areaSegmentCenterMap[id.Value];
            foreach (var neighbor in VoronoiDiagram.NeighborSitesForSite(new Vector2f(center.x, center.y)))
            {
                var neighborSegment = AreaSegmentGraph.GetNodeData(_siteAreaMap[neighbor]);
                if (neighborSegment.Type != AreaSegment.EAreaSegmentType.Border)
                {
                    AreaSegmentGraph.AddEdge(_siteAreaMap[neighbor], id.Value, (int) AreaSegment.EAreaSegmentEdgeType.NonNavigable);
                }
            }
        }
    }

    /* Assign areas to the base graph based on a specific set of rules */
    private void CreateAreaGraph(StoryStructure storyStructure)
    {
        // TODO: Graph grammar transformations - assign main path, side paths, boss area, start area, special areas...
        // TODO: Build grammar graph with set of rules
        while (storyStructure.Rewrites.Count > 0)
        {
            var rule = storyStructure.Rewrites.Dequeue();
            if(!AreaSegmentGraph.Replace(rule.Pattern, rule.Replace, rule.Correspondences))
                throw new Exception("Failed to generate");
        }
    }

    /* Generates a polygon for a given voronoi site */
    private Vector2[] GenerateSitePolygon(Vector2f site)
    {
        var edges = VoronoiDiagram.SitesIndexedByLocation[site].Edges;
        if (edges == null || edges.Count <= 0)
        {
            Debug.Log("Could not build polygon for site " + site);
            return null;
        }

        var result = new List<Vector2>(edges.Count);
        var reorderer = new EdgeReorderer(edges, typeof(Vertex));
        for (var i = 0; i < reorderer.Edges.Count; i++)
        {
            var edge = reorderer.Edges[i];
            if (!edge.Visible()) continue;

            result.Add(edge.ClippedEnds[reorderer.EdgeOrientations[i]].ToUnityVector2());
        }

        return result.ToArray();
    }

    /* Get closest AreaSegment center to specified pos */
    private AreaSegment GetClosestAreaSegment(Vector2 pos)
    {
        float smallestDistance = float.MaxValue;
        AreaSegment closestAreaSegment = null;
        foreach (var id in AreaSegmentGraph.GetAllNodeIDs())
        {
            Vector2 center = _areaSegmentCenterMap[id];
            float currentDistance = (center - pos).sqrMagnitude;
            if (currentDistance < smallestDistance)
            {
                smallestDistance = currentDistance;
                closestAreaSegment = AreaSegmentGraph.GetNodeData(id);
            }
        }

        return closestAreaSegment;
    }

    private Texture2D GenerateBlankNormal()
    {
        var texture = new Texture2D(16, 16, TextureFormat.ARGB32, false);
        var cols = texture.GetPixels32(0);
        var colsLength = cols.Length;
        for (var i = 0; i < colsLength; i++)
        {
            cols[i] = new Color(.5f, .5f, 1, 1);
        }
        texture.SetPixels32(cols, 0);
        texture.Apply(false);
        texture.Compress(false);
        return texture;
    }

    private Texture2D GenerateBlankSpec()
    {
        var texture = new Texture2D(16, 16, TextureFormat.RGB24, false);
        var cols = texture.GetPixels(0);
        var colsLength = cols.Length;
        for (var i = 0; i < colsLength; i++)
        {
            cols[i] = new Color(0.1f, 0.1f, 0, 0);
        }
        texture.SetPixels(cols, 0);
        texture.Apply(false);
        texture.Compress(false);
        return texture;
    }
}