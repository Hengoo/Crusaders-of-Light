using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using csDelaunay;


public class TerrainStructure
{
    
    public Voronoi VoronoiDiagram { get; private set; }
    public Graph<Biome> BiomeGraph { get; private set; }
    public Graph<Biome> MinimumSpanningTree { get; private set; }
    public readonly BiomeConfiguration BiomeConfiguration;
    public KeyValuePair<Vector2f, int> StartBiomeNode;

    private readonly Dictionary<Vector2f, int> _siteBiomeMap = new Dictionary<Vector2f, int>(); //Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<SplatPrototypeSerializable, int> _splatIDMap = new Dictionary<SplatPrototypeSerializable, int>(); //Mapping of biome SplatPrototypes and terrain texture IDs

    public int TextureCount { get { return _splatIDMap.Count; } }

    private Texture2D _blankSpec;
    private Texture2D _blankBump;

    public TerrainStructure(List<BiomeSettings> availableBiomes, BiomeConfiguration biomeConfiguration)
    {
        BiomeGraph = new Graph<Biome>();

        BiomeConfiguration = biomeConfiguration;

        //Add Splat textures to global shader variables
        _blankBump = GenerateBlankNormal();
        _blankSpec = GenerateBlankSpec();
        var count = 0;
        foreach (var biome in availableBiomes)
        {
            if (_splatIDMap.ContainsKey(biome.Splat))
                continue;

            _splatIDMap.Add(biome.Splat, count);

            Shader.SetGlobalTexture("_BumpMap" + count, biome.Splat.normalMap ? biome.Splat.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1/biome.Splat.tileSize.x);
            count++;
        }

        //Add border biome to the SplatPrototypes map
        if (!_splatIDMap.ContainsKey(BiomeConfiguration.BorderBiome.Splat))
        {
            _splatIDMap.Add(BiomeConfiguration.BorderBiome.Splat, count);
            Shader.SetGlobalTexture("_BumpMap" + count, BiomeConfiguration.BorderBiome.Splat.normalMap ? BiomeConfiguration.BorderBiome.Splat.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1/BiomeConfiguration.BorderBiome.Splat.tileSize.x);
        }


        var navigableBiomeIDs = new HashSet<int>();
        var centers = new List<Vector2f>();

        // Create random point distribution and apply lloyd relaxation
        for (int i = 0; i < biomeConfiguration.BiomeSamples; i++)
        {
            var x = Random.Range(0f, biomeConfiguration.MapSize);
            var y = Random.Range(0f, biomeConfiguration.MapSize);
            centers.Add(new Vector2f(x, y));
        }

        VoronoiDiagram = new Voronoi(centers, new Rectf(0, 0, biomeConfiguration.MapSize, biomeConfiguration.MapSize));
        VoronoiDiagram.LloydRelaxation(biomeConfiguration.LloydRelaxation);
        

        //Iterate over each site and add a biome to it
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

            /* Assign biome to site - water if on border */
            var biome = isOnBorder
                ? new Biome(site.ToUnityVector2(), BiomeConfiguration.BorderBiome, true, null)
                : new Biome(site.ToUnityVector2(), availableBiomes[Random.Range(0, availableBiomes.Count)], false, GenerateSitePolygon(site));

            var biomeID = BiomeGraph.AddNode(biome);
            _siteBiomeMap.Add(site, biomeID);
            if (!biome.BiomeSettings.NotNavigable)
                navigableBiomeIDs.Add(biomeID);
        }

        /* MSP */
        MinimumSpanningTree = new Graph<Biome>(BiomeGraph);
        foreach (var edge in GeneratePaths())
        {
            MinimumSpanningTree.AddEdge(edge.Value, edge.Key, 1);
        }

        /* Create navigation graph - for each biome, add reachable neighbors */
        foreach (var id in _siteBiomeMap)
        {
            var biome = BiomeGraph.GetNodeData(id.Value);
            if (biome.BiomeSettings.NotNavigable) continue;

            foreach (var neighbor in VoronoiDiagram.NeighborSitesForSite(new Vector2f(biome.Center.x, biome.Center.y)))
            {
                var neighborBiome = BiomeGraph.GetNodeData(_siteBiomeMap[neighbor]);
                if (!neighborBiome.BiomeSettings.NotNavigable)
                {
                    BiomeGraph.AddEdge(_siteBiomeMap[neighbor], id.Value, 1);
                }
            }
        }
    }

    // Returns all biomes' polygons
    public List<Vector2[]> GetBiomePolygons(out List<GameObject[]> prefabs, out List<float> minDistances)
    {
        var result = new List<Vector2[]>();
        prefabs = new List<GameObject[]>();
        minDistances = new List<float>();
        foreach (var siteBiome in _siteBiomeMap)
        {
            var biome = BiomeGraph.GetNodeData(siteBiome.Value);
            if (biome.IsBorderBiome)
                continue;

            result.Add(biome.BiomePolygon);
            prefabs.Add(biome.BiomeSettings.FillPrefabs);
            minDistances.Add(biome.BiomeSettings.PrefabMinDistance);
        }

        return result;
    }

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
            //if (i == reorderer.Edges.Count - 1)
            //    result.Add(edge.ClippedEnds[reorderer.EdgeOrientations[i] == LR.RIGHT ? LR.LEFT : LR.RIGHT].ToUnityVector2());
        }

        return result.ToArray();
    }


    // Returns a sorted list of the textures
    public IEnumerable<Texture> GetTerrainTextures()
    {
        var result = new SortedList<int, Texture>();

        foreach (var splatID in _splatIDMap)
        {
            result.Add(splatID.Value, splatID.Key.texture);
        }

        return result.Values;
    }

    // Returns a sorted list of the splats
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

    public IEnumerable<LineSegment> GetBiomeSmoothBorders()
    {
        var result = new List<LineSegment>();

        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftBiome = BiomeGraph.GetNodeData(_siteBiomeMap[edge.LeftSite.Coord]);
            var rightBiome = BiomeGraph.GetNodeData(_siteBiomeMap[edge.RightSite.Coord]);
            if (leftBiome.BiomeSettings.UniqueName == rightBiome.BiomeSettings.UniqueName
                || leftBiome.BiomeSettings.DontBlendWith.Contains(rightBiome.BiomeSettings)
                || rightBiome.BiomeSettings.DontBlendWith.Contains(leftBiome.BiomeSettings))

                continue;

            var p0 = edge.ClippedEnds[LR.LEFT];
            var p1 = edge.ClippedEnds[LR.RIGHT];
            var segment = new LineSegment(p0, p1);
            result.Add(segment);
        }
        //DrawLineSegments(result, 1, new GameObject("Blended Borders").transform);

        return result;
    }

    public IEnumerable<LineSegment> GetBiomeBorders()
    {
        var result = new List<LineSegment>();

        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftBiome = BiomeGraph.GetNodeData(_siteBiomeMap[edge.LeftSite.Coord]);
            var rightBiome = BiomeGraph.GetNodeData(_siteBiomeMap[edge.RightSite.Coord]);
            if (leftBiome.BiomeSettings.UniqueName == rightBiome.BiomeSettings.UniqueName)
                continue;

            var p0 = edge.ClippedEnds[LR.LEFT];
            var p1 = edge.ClippedEnds[LR.RIGHT];
            var segment = new LineSegment(p0, p1);
            result.Add(segment);
        }
        //DrawLineSegments(result, 1, new GameObject("All Borders").transform);

        return result;
    }

    public BiomeHeight SampleBiomeHeight(Vector2 position)
    {
        var pos = new Vector2f(position.x + Random.Range(-BiomeConfiguration.BorderNoise, BiomeConfiguration.BorderNoise),
            position.y + Random.Range(-BiomeConfiguration.BorderNoise, BiomeConfiguration.BorderNoise));
        var closestBiome = GetClosestBiome(pos);

        return closestBiome == null ? BiomeConfiguration.BorderBiome.Height : closestBiome.BiomeSettings.Height;
    }

    public IEnumerable<KeyValuePair<int, float>> SampleBiomeTexture(Vector2 position)
    {
        var pos = new Vector2f(position.x + Random.Range(-BiomeConfiguration.BorderNoise, BiomeConfiguration.BorderNoise) * 1.2f,
            position.y + Random.Range(-BiomeConfiguration.BorderNoise, BiomeConfiguration.BorderNoise) * 1.2f);
        var closestBiome = GetClosestBiome(pos);
        var result = new List<KeyValuePair<int, float>>
        {
            new KeyValuePair<int, float>(_splatIDMap[closestBiome.BiomeSettings.Splat], 1)
        };

        return result;
    }

    private Biome GetClosestBiome(Vector2f position)
    {
        Biome result = null;
        var closestSqrDistance = float.MaxValue;
        foreach (var biome in _siteBiomeMap)
        {
            var currentBiome = BiomeGraph.GetNodeData(biome.Value);
            var center = new Vector2f(currentBiome.Center.x, currentBiome.Center.y);
            var sqrDistance = center.DistanceSquare(position);
            if (sqrDistance < closestSqrDistance)
            {
                result = BiomeGraph.GetNodeData(biome.Value);
                closestSqrDistance = sqrDistance;
            }
        }

        return result;
    }

    public GameObject DrawBiomeGraph(float scale)
    {
        var result = new GameObject();

        var biomes = new GameObject("Biomes");
        biomes.transform.parent = result.transform;
        var voronoi = new GameObject("Voronoi");
        voronoi.transform.parent = result.transform;
        var delaunay = new GameObject("Modified Delaunay");
        delaunay.transform.parent = result.transform;

        foreach (var biome in _siteBiomeMap)
        {
            var pos = new Vector2(biome.Key.x, biome.Key.y);
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Biome id: " + biome.Value;
            go.GetComponent<Collider>().enabled = false;
            go.transform.parent = biomes.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20 * scale;
            if (biome.Value == StartBiomeNode.Value)
            {
                var renderer = go.GetComponent<Renderer>();
                var tempMaterial = new Material(renderer.sharedMaterial) { color = Color.red };
                renderer.sharedMaterial = tempMaterial;
            }
        }

        DrawLineSegments(VoronoiDiagram.VoronoiDiagram(), scale, voronoi.transform);

        foreach (var edge in BiomeGraph.GetAllEdges())
        {
            var biome1 = BiomeGraph.GetNodeData(edge.x);
            var biome2 = BiomeGraph.GetNodeData(edge.y);

            var start = new Vector3(biome1.Center.x, 0, biome1.Center.y);
            var end = new Vector3(biome2.Center.x, 0, biome2.Center.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = delaunay.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        return result;
    }

    private void DrawLineSegments(IEnumerable<LineSegment> lines, float scale, Transform parent)
    {
        foreach (var line in lines)
        {
            var start = new Vector3(line.p0.x, 0, line.p0.y);
            var end = new Vector3(line.p1.x, 0, line.p1.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = parent;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    /* Generate paths between existing biomes */
    private List<KeyValuePair<int, int>> GeneratePaths()
    {
        List<KeyValuePair<int, int>> result;
        var navigableBiomes = new Dictionary<Vector2f, int>();
        var randomBiomeList = new List<KeyValuePair<Vector2f, int>>();
        foreach (var pair in _siteBiomeMap)
        {
            if (!BiomeGraph.GetNodeData(pair.Value).BiomeSettings.NotNavigable)
            {
                navigableBiomes.Add(pair.Key, pair.Value);
                randomBiomeList.Add(pair);
            }
        }

        //Less biased towards outer biomes than using unity's random function
        randomBiomeList.Shuffle();
        StartBiomeNode = randomBiomeList.First();

        result = PrimMSP(StartBiomeNode, navigableBiomes);

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



