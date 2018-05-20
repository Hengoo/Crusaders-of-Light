using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using csDelaunay;


public class TerrainStructure
{

    public Voronoi VoronoiDiagram { get; private set; }
    public Graph<Biome> BaseGraph { get; private set; }
    public Graph<Biome> AreaGraph { get; private set; }
    public KeyValuePair<Vector2f, int> StartBiomeNode;
    public KeyValuePair<Vector2f, int> BossBiomeNode;

    public int TextureCount { get { return _splatIDMap.Count; } }

    public float MapSize { get; private set; }
    public int HeightMapResolution { get; private set; }
    public int Octaves { get; private set; }
    public int RoadSplatIndex { get; private set; }

    public readonly List<Vector2> OuterBorderPolygon;
    public readonly List<Vector2[]> RoadLines;
    public readonly List<Vector2[]> AreaBorders;

    private readonly Dictionary<Vector2f, int> _siteBiomeMap = new Dictionary<Vector2f, int>(); //Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<SplatPrototypeSerializable, int> _splatIDMap = new Dictionary<SplatPrototypeSerializable, int>(); //Mapping of biome SplatPrototypes and terrain texture IDs

    private Texture2D _blankSpec;
    private Texture2D _blankBump;

    private readonly BiomeSettings _borderSettings;
    private readonly SplatPrototypeSerializable _roadSplatPrototype;
    private readonly int _voronoiSamples;
    private readonly float _borderNoise;

    public TerrainStructure(StoryStructure storyStructure, BiomeGlobalConfiguration globalConfiguration)
    {
        BaseGraph = new Graph<Biome>();
        OuterBorderPolygon = new List<Vector2>();
        AreaBorders = new List<Vector2[]>();
        RoadLines = new List<Vector2[]>();

        MapSize = globalConfiguration.MapSize;
        HeightMapResolution = globalConfiguration.HeightMapResolution;
        Octaves = globalConfiguration.Octaves;

        _borderSettings = globalConfiguration.BorderBiome;
        _roadSplatPrototype = globalConfiguration.RoadSplatPrototype;
        _voronoiSamples = globalConfiguration.VoronoiSamples;
        _borderNoise = globalConfiguration.BorderNoise;

        // Add splat prototypes to the shader
        AddShaderTextures(storyStructure);

        // Create base graph that later on is transformed with a set of rules and assigned areas to
        CreateBaseGraph(storyStructure, globalConfiguration);

        // Assign specific areas to each node of the base graph - Start point, Boss arena, paths...
        //TODO: Graph grammar transformations - assign main path, side paths, boss area, start area, special areas...
        CreateAreaGraph();
    }

    // Returns all biomes' polygons
    public List<Vector2[]> GetBiomePolygons(out List<GameObject[]> prefabs, out List<float> minDistances)
    {
        var result = new List<Vector2[]>();
        prefabs = new List<GameObject[]>();
        minDistances = new List<float>();
        foreach (var siteBiome in _siteBiomeMap)
        {
            var biome = BaseGraph.GetNodeData(siteBiome.Value);
            if (biome.IsBorderBiome)
                continue;

            result.Add(biome.BiomePolygon);
            prefabs.Add(biome.BiomeSettings.FillPrefabs);
            minDistances.Add(biome.BiomeSettings.PrefabMinDistance);
        }

        return result;
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

    /* Get all borders that should not be smoothed between biomes */
    public IEnumerable<Vector2[]> GetNonBlendingBiomeBorders()
    {
        var result = new List<Vector2[]>();

        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftBiome = BaseGraph.GetNodeData(_siteBiomeMap[edge.LeftSite.Coord]);
            var rightBiome = BaseGraph.GetNodeData(_siteBiomeMap[edge.RightSite.Coord]);
            if (leftBiome.BiomeSettings.UniqueName == rightBiome.BiomeSettings.UniqueName
                || leftBiome.BiomeSettings.DontBlendWith.Contains(rightBiome.BiomeSettings)
                || rightBiome.BiomeSettings.DontBlendWith.Contains(leftBiome.BiomeSettings))

                continue;

            var p0 = edge.ClippedEnds[LR.LEFT].ToUnityVector2();
            var p1 = edge.ClippedEnds[LR.RIGHT].ToUnityVector2();
            var segment = new[] { p0, p1 };
            result.Add(segment);
        }

        return result;
    }

    /* Get all biome borders */
    public IEnumerable<LineSegment> GetBiomeBorders()
    {
        var result = new List<LineSegment>();

        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftBiome = BaseGraph.GetNodeData(_siteBiomeMap[edge.LeftSite.Coord]);
            var rightBiome = BaseGraph.GetNodeData(_siteBiomeMap[edge.RightSite.Coord]);
            if (leftBiome.BiomeSettings.UniqueName == rightBiome.BiomeSettings.UniqueName)
                continue;

            var p0 = edge.ClippedEnds[LR.LEFT];
            var p1 = edge.ClippedEnds[LR.RIGHT];
            var segment = new LineSegment(p0, p1);
            result.Add(segment);
        }

        return result;
    }

    /* Sample biome height at a given position */
    public BiomeHeight SampleBiomeHeight(Vector2 position)
    {
        var pos = new Vector2f(position.x + Random.Range(-_borderNoise, _borderNoise),
            position.y + Random.Range(-_borderNoise, _borderNoise));
        var closestBiome = GetClosestBiome(pos);

        return closestBiome == null ? _borderSettings.Height : closestBiome.BiomeSettings.Height;
    }

    /* Sample biome texture at a given position */
    public IEnumerable<KeyValuePair<int, float>> SampleBiomeTexture(Vector2 position)
    {
        var pos = new Vector2f(position.x + Random.Range(-_borderNoise, _borderNoise) * 1.2f,
            position.y + Random.Range(-_borderNoise, _borderNoise) * 1.2f);
        var closestBiome = GetClosestBiome(pos);
        var result = new List<KeyValuePair<int, float>>
        {
            new KeyValuePair<int, float>(_splatIDMap[closestBiome.BiomeSettings.Splat], 1)
        };

        return result;
    }

    /* Return biome ID from a csDelaunay vector */
    public int GetNodeIDFromSite(Vector2f coord)
    {
        return _siteBiomeMap[coord];
    }

    //---------------------------------------------------------------
    //
    //              HELPER FUNCTIONS
    //
    //---------------------------------------------------------------

    /* Assign textures to shader */
    private void AddShaderTextures(StoryStructure storyStructure)
    {
        // Add Splat textures to global shader variables
        _blankBump = GenerateBlankNormal();
        _blankSpec = GenerateBlankSpec();
        var count = 0;

        _splatIDMap.Add(storyStructure.Biome.Splat, count);
        Shader.SetGlobalTexture("_BumpMap" + count, storyStructure.Biome.Splat.normalMap ? storyStructure.Biome.Splat.normalMap : _blankBump);
        Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
        Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / storyStructure.Biome.Splat.tileSize.x);
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
    private void CreateBaseGraph(StoryStructure storyStructure, BiomeGlobalConfiguration globalConfiguration)
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
        VoronoiDiagram.LloydRelaxation(globalConfiguration.LloydRelaxation);

        // Assign base areas to each site
        var navigableBiomeIDs = new HashSet<int>();
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

            // Assign biome to site - water if on border
            var biome = isOnBorder
                ? new Biome(site.ToUnityVector2(), _borderSettings, true, null)
                : new Biome(site.ToUnityVector2(), storyStructure.Biome, false, GenerateSitePolygon(site));

            var biomeID = BaseGraph.AddNode(biome);
            _siteBiomeMap.Add(site, biomeID);
            if (!biome.BiomeSettings.NotNavigable)
                navigableBiomeIDs.Add(biomeID);
        }
    }

    /* Assign areas to the base graph based on a specific set of rules */
    private void CreateAreaGraph()
    {
        // Build the minimum spanning tree
        AreaGraph = new Graph<Biome>(BaseGraph);
        foreach (var edge in GeneratePaths())
        {
            AreaGraph.AddEdge(edge.Value, edge.Key, 1);
        }

        // Create path - for each area, add reachable neighbors
        foreach (var id in _siteBiomeMap)
        {
            var area = BaseGraph.GetNodeData(id.Value);
            if (area.BiomeSettings.NotNavigable) continue;

            foreach (var neighbor in VoronoiDiagram.NeighborSitesForSite(new Vector2f(area.Center.x, area.Center.y)))
            {
                var neighborBiome = BaseGraph.GetNodeData(_siteBiomeMap[neighbor]);
                if (!neighborBiome.BiomeSettings.NotNavigable)
                {
                    BaseGraph.AddEdge(_siteBiomeMap[neighbor], id.Value, 1);
                }
            }
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

    /* Get biome closest to the given position */
    private Biome GetClosestBiome(Vector2f position)
    {
        Biome result = null;
        var closestSqrDistance = float.MaxValue;
        foreach (var biome in _siteBiomeMap)
        {
            var currentBiome = BaseGraph.GetNodeData(biome.Value);
            var center = new Vector2f(currentBiome.Center.x, currentBiome.Center.y);
            var sqrDistance = center.DistanceSquare(position);
            if (sqrDistance < closestSqrDistance)
            {
                result = BaseGraph.GetNodeData(biome.Value);
                closestSqrDistance = sqrDistance;
            }
        }

        return result;
    }

    /* Generate paths between existing biomes */
    private List<KeyValuePair<int, int>> GeneratePaths()
    {
        var navigableBiomes = new Dictionary<Vector2f, int>();
        var randomBiomeList = new List<KeyValuePair<Vector2f, int>>();

        // Get all the navigable biomes list
        foreach (var pair in _siteBiomeMap)
        {
            if (!BaseGraph.GetNodeData(pair.Value).BiomeSettings.NotNavigable)
            {
                navigableBiomes.Add(pair.Key, pair.Value);
                randomBiomeList.Add(pair);
            }
        }

        // Less biased towards outer biomes than using unity's random function
        randomBiomeList.Shuffle();
        StartBiomeNode = randomBiomeList.First();

        // Construct the minimum spanning tree using Prim
        var result = PrimMSP(StartBiomeNode, navigableBiomes);
        return result;
    }

    /* Create a Minimum Spanning Tree using Prim's algorithm */
    private static List<KeyValuePair<int, int>> PrimMSP(KeyValuePair<Vector2f, int> startNode, IDictionary<Vector2f, int> nodes)
    {
        var result = new List<KeyValuePair<int, int>>();
        var tree = new List<KeyValuePair<Vector2f, int>>();
        nodes.Remove(startNode.Key);
        tree.Add(startNode);

        //Iterate until all nodes all connected to the tree
        while (nodes.Count > 0)
        {
            var current = new KeyValuePair<Vector2f, int>();
            var closest = new KeyValuePair<Vector2f, int>();
            float closestSqrDistance = float.MaxValue;

            //Find the closest node pair, where one node is in the tree and the other isn't
            foreach (var node in tree)
            {
                foreach (var outNode in nodes)
                {
                    var currentDistance = node.Key.DistanceSquare(outNode.Key);
                    if (currentDistance < closestSqrDistance)
                    {
                        closest = node;
                        current = outNode;
                        closestSqrDistance = currentDistance;
                    }
                }
            }

            nodes.Remove(current.Key);
            tree.Add(current);
            result.Add(new KeyValuePair<int, int>(current.Value, closest.Value));
        }

        return result;
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