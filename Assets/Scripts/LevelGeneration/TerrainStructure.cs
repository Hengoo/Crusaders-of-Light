using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using UnityEngine;
using Random = UnityEngine.Random;
using csDelaunay;
// ReSharper disable PossibleMultipleEnumeration


public class TerrainStructure
{

    public Voronoi VoronoiDiagram { get; private set; }
    public GrammarGraph<AreaSegment> AreaSegmentGraph { get; private set; }
    public KeyValuePair<Vector2f, int> StartAreaSegment { get; private set; }
    public BiomeSettings BiomeSettings { get; private set; }
    public BiomeSettings BorderSettings { get; private set; }

    public int TextureCount { get { return _splatIDMap.Count; } }

    public float MapSize { get; private set; }
    public int HeightMapResolution { get; private set; }
    public int Octaves { get; private set; }
    public int MainPathSplatIndex { get; private set; }
    public int SidePathSplatIndex { get; private set; }

    public readonly List<Vector2[]> BorderBlockerLines;
    public readonly List<Vector2[]> AreaBlockerLines;
    public readonly List<Vector2[]> MainPathLines;
    public readonly List<Vector2[]> SidePathLines;
    public readonly List<Vector2[]> PathPolygons;

    private readonly Dictionary<int, Vector2> _areaSegmentCenterMap = new Dictionary<int, Vector2>(); // Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<Vector2f, int> _siteAreaSegmentMap = new Dictionary<Vector2f, int>(); // Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<SplatPrototypeSerializable, int> _splatIDMap = new Dictionary<SplatPrototypeSerializable, int>(); // Mapping of biome SplatPrototypes and terrain texture IDs

    private Texture2D _blankSpec;
    private Texture2D _blankBump;

    private readonly int _voronoiSamples;
    private readonly float _borderNoise;

    public TerrainStructure(StoryStructure storyStructure, List<BiomeSettings> availableBiomes, float mapSize,
        int heightMapResolution, int octaves, BiomeSettings borderSettings,
        int voronoiSamples, int lloydIterations, float edgeNoise, float borderBlockerOffset)
    {
        AreaSegmentGraph = new GrammarGraph<AreaSegment>();
        BorderBlockerLines = new List<Vector2[]>();
        AreaBlockerLines = new List<Vector2[]>();
        MainPathLines = new List<Vector2[]>();
        SidePathLines = new List<Vector2[]>();
        PathPolygons = new List<Vector2[]>();

        // Select a random biome out of the available ones for the current level
        BiomeSettings = availableBiomes[Random.Range(0, availableBiomes.Count)];

        MapSize = mapSize;
        HeightMapResolution = heightMapResolution;
        Octaves = octaves;

        BorderSettings = borderSettings;
        _voronoiSamples = voronoiSamples;
        _borderNoise = edgeNoise;

        // Add splat prototypes to the shader
        CreateShaderTextures();

        // Create base graph that later on is transformed with a set of rules and assigned areas to
        CreateBaseGraph(lloydIterations);

        // Assign specific areas to each node of the base graph - Start point, Boss arena, paths...
        CreateAreaGraph(storyStructure.Rewrites);

        // Populate path lines list
        CreatePathLines();

        // Populate area segment blockers list
        CreateAreaBlockerLines();

        // Populate border lines list
        CreateBorderBlockerPolygon(borderBlockerOffset);

        // Create path polygons
        CreatePathPolygons(BiomeSettings.PathHalfWidth);
    }

    // Returns the center position for a given area segment ID
    public Vector2 GetAreaSegmentCenter(int id)
    {
        Vector2 value;
        return _areaSegmentCenterMap.TryGetValue(id, out value) ? value : Vector2.zero;
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

    // Sample area height at a given position 
    public BiomeHeightParameters SampleHeight(Vector2 position)
    {
        Vector2 noisePosition = position + new Vector2(Random.Range(-_borderNoise, _borderNoise), Random.Range(-_borderNoise, _borderNoise));
        AreaSegment areaSegment = GetClosestAreaSegment(noisePosition);
        return areaSegment == null || areaSegment.Type == AreaSegment.EAreaSegmentType.Border ? BorderSettings.HeightParameters : BiomeSettings.HeightParameters;
    }

    // Sample area texture at given position
    public KeyValuePair<int, float> SampleTexture(Vector2 position)
    {
        Vector2 noisePosition = position + new Vector2(Random.Range(-_borderNoise, _borderNoise), Random.Range(-_borderNoise, _borderNoise));
        AreaSegment areaSegment = GetClosestAreaSegment(noisePosition);
        int splatID = _splatIDMap[areaSegment.Type == AreaSegment.EAreaSegmentType.Border ? BorderSettings.Splat : BiomeSettings.Splat];
        float splatValue = 1;
        return new KeyValuePair<int, float>(splatID, splatValue);
    }

    // Get area data graph that contains the given area segments, with the respective center and polygon, and neighborhood information
    public Graph<AreaData> GetAreaDataGraph(IEnumerable<int> areaSegments)
    {
        if (!AreNeighbours(areaSegments))
            return null;

        // Build polygons for each site
        Dictionary<int, Vector2[]> areaSegmentPolygonMap = new Dictionary<int, Vector2[]>();
        foreach (int areaSegment in areaSegments)
        {
            var site = new Vector2f(_areaSegmentCenterMap[areaSegment]);
            areaSegmentPolygonMap.Add(areaSegment, GetSitePolygon(site));
        }

        // Build graph
        Dictionary<int, int> graphMap = new Dictionary<int, int>();
        Graph<AreaData> areaDataGraph = new Graph<AreaData>();
        foreach (int areaSegment in areaSegments)
        {
            AreaData data = new AreaData
            {
                Center = _areaSegmentCenterMap[areaSegment],
                Polygon = areaSegmentPolygonMap[areaSegment],
                Segment = AreaSegmentGraph.GetNodeData(areaSegment)
            };
            int newID = areaDataGraph.AddNode(data);
            graphMap.Add(areaSegment, newID);
        }
        foreach (int areaSegment in areaSegments)
        {
            foreach (int neighbour in AreaSegmentGraph.GetNeighbours(areaSegment))
            {
                if (areaSegments.Contains(neighbour))
                    areaDataGraph.AddEdge(graphMap[areaSegment], graphMap[neighbour], 0);
            }
        }

        return areaDataGraph;
    }

    // Get path polygons that have vertices inside areas segments
    public List<Vector2[]> GetPathPolygons(IEnumerable<int> areaSegments)
    {
        //TODO: implement this
        return null;
    }

    // Get border polygon for a given set of area segments
    public Vector2[] GetAreaSegmentsBorderPolygon(IEnumerable<int> areaSegments)
    {
        var areaEdges = new List<Edge>();

        // Get corresponding edges
        foreach (var edge in VoronoiDiagram.Edges)
        {
            //Check if this edge is visible before continuing
            if (!edge.Visible()) continue;

            int leftAreaSegmentID = _siteAreaSegmentMap[edge.RightSite.Coord];
            int rightAreaSegmentID = _siteAreaSegmentMap[edge.LeftSite.Coord];

            // Either one or the other must be in areaSegments
            if (!(areaSegments.Contains(leftAreaSegmentID) && areaSegments.Contains(rightAreaSegmentID)) &&
                (areaSegments.Contains(leftAreaSegmentID) || areaSegments.Contains(rightAreaSegmentID)))
            {
                areaEdges.Add(edge);
            }
        }

        // Group connected segments
        var polygon = new List<Vector2>();
        while (areaEdges.Count > 0)
        {
            var startEdge = areaEdges[0];
            areaEdges.Remove(startEdge);
            Vertex headPoint = startEdge.RightVertex;
            Vertex tailPoint = startEdge.LeftVertex;

            // Find polygon
            var polygonClosed = false;
            while (!polygonClosed && areaEdges.Count > 0)
            {
                for (int i = 0; i < areaEdges.Count; i++)
                {
                    var currentElement = areaEdges[i];
                    Vertex leftPoint = currentElement.LeftVertex;
                    Vertex rightPoint = currentElement.RightVertex;
                    if (leftPoint == headPoint)
                    {
                        areaEdges.Remove(currentElement);
                        headPoint = rightPoint;
                        polygon.Add(currentElement.ClippedEnds[LR.RIGHT].ToUnityVector2());
                    }
                    else if (rightPoint == headPoint)
                    {
                        areaEdges.Remove(currentElement);
                        headPoint = leftPoint;
                        polygon.Add(currentElement.ClippedEnds[LR.LEFT].ToUnityVector2());
                    }

                    // Polygon has been closed
                    if (headPoint == tailPoint)
                    {
                        polygonClosed = true;
                        break;
                    }
                }
            }
        }

        return polygon.ToArray();
    }

    //---------------------------------------------------------------
    //
    //              CREATOR FUNCTIONS
    //
    //---------------------------------------------------------------

    // Assign textures to shader
    private void CreateShaderTextures()
    {
        // Add Splat textures to global shader variables
        _blankBump = GenerateBlankNormal();
        _blankSpec = GenerateBlankSpec();
        var count = 0;

        // Add main biome to the SplatPrototypes map
        _splatIDMap.Add(BiomeSettings.Splat, count);
        Shader.SetGlobalTexture("_BumpMap" + count, BiomeSettings.Splat.normalMap ? BiomeSettings.Splat.normalMap : _blankBump);
        Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
        Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / BiomeSettings.Splat.tileSize.x);
        count++;


        // Add border biome to the SplatPrototypes map
        if (!_splatIDMap.ContainsKey(BorderSettings.Splat))
        {
            _splatIDMap.Add(BorderSettings.Splat, count);

            Shader.SetGlobalTexture("_BumpMap" + count, BorderSettings.Splat.normalMap ? BorderSettings.Splat.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / BorderSettings.Splat.tileSize.x);
            count++;
        }

        // Add main path to the SplatPrototypes map
        if (!_splatIDMap.ContainsKey(BiomeSettings.MainPathSplatPrototype))
        {
            _splatIDMap.Add(BiomeSettings.MainPathSplatPrototype, count);

            Shader.SetGlobalTexture("_BumpMap" + count, BiomeSettings.MainPathSplatPrototype.normalMap ? BiomeSettings.MainPathSplatPrototype.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / BiomeSettings.MainPathSplatPrototype.tileSize.x);
            MainPathSplatIndex = count;
            count++;
        }

        // Add side path to the SplatPrototypes map
        if (!_splatIDMap.ContainsKey(BiomeSettings.SidePathSplatPrototype))
        {
            _splatIDMap.Add(BiomeSettings.SidePathSplatPrototype, count);

            Shader.SetGlobalTexture("_BumpMap" + count, BiomeSettings.SidePathSplatPrototype.normalMap ? BiomeSettings.SidePathSplatPrototype.normalMap : _blankBump);
            Shader.SetGlobalTexture("_SpecMap" + count, _blankSpec);
            Shader.SetGlobalFloat("_TerrainTexScale" + count, 1 / BiomeSettings.SidePathSplatPrototype.tileSize.x);
            SidePathSplatIndex = count;
        }
    }

    // Create a graph containing all connected empty areas
    private void CreateBaseGraph(int lloydIterations)
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
        VoronoiDiagram.LloydRelaxation(lloydIterations);

        // Assign area segments to initial areas
        foreach (var site in VoronoiDiagram.SiteCoords())
        {
            bool isOnBorder = false;
            var segments = VoronoiDiagram.VoronoiBoundaryForSite(site);

            foreach (var segment in segments)
            {
                if (!(segment.p0.x <= VoronoiDiagram.PlotBounds.left) &&
                    !(segment.p0.x >= VoronoiDiagram.PlotBounds.right) &&
                    !(segment.p0.y <= VoronoiDiagram.PlotBounds.top) &&
                    !(segment.p0.y >= VoronoiDiagram.PlotBounds.bottom) &&
                    !(segment.p1.x <= VoronoiDiagram.PlotBounds.left) &&
                    !(segment.p1.x >= VoronoiDiagram.PlotBounds.right) &&
                    !(segment.p1.y <= VoronoiDiagram.PlotBounds.top) &&
                    !(segment.p1.y >= VoronoiDiagram.PlotBounds.bottom))
                    continue;

                isOnBorder = true;
                break;
            }

            // Assign areaSegment to site and corresponding area
            var areaSegment = new AreaSegment(isOnBorder ? AreaSegment.EAreaSegmentType.Border : AreaSegment.EAreaSegmentType.Empty);

            var nodeID = AreaSegmentGraph.AddNode(areaSegment);
            _siteAreaSegmentMap.Add(site, nodeID);
            _areaSegmentCenterMap.Add(nodeID, site.ToUnityVector2());
        }

        // Create navigation graph - for each area segment that is not a border, add reachable neighbors
        foreach (var id in _siteAreaSegmentMap)
        {
            var areaSegment = AreaSegmentGraph.GetNodeData(id.Value);
            if (areaSegment.Type == AreaSegment.EAreaSegmentType.Border) continue;

            Vector2 center = _areaSegmentCenterMap[id.Value];
            foreach (var neighbor in VoronoiDiagram.NeighborSitesForSite(new Vector2f(center.x, center.y)))
            {
                var neighborSegment = AreaSegmentGraph.GetNodeData(_siteAreaSegmentMap[neighbor]);
                if (neighborSegment.Type != AreaSegment.EAreaSegmentType.Border)
                {
                    AreaSegmentGraph.AddEdge(_siteAreaSegmentMap[neighbor], id.Value, (int)AreaSegment.EAreaSegmentEdgeType.NonNavigable);
                }
            }
        }
    }

    // Assign areas to the base graph based on a specific set of rules
    private void CreateAreaGraph(Queue<StoryStructure.AreaSegmentRewrite> rewrites)
    {
        while (rewrites.Count > 0)
        {
            var rule = rewrites.Dequeue();
            if (!AreaSegmentGraph.Replace(rule.Pattern, rule.Replace, rule.Correspondences))
            {
                Debug.LogWarning("Failed to generate map with current seed: " + Random.state);
                Random.InitState(Random.Range(0, int.MaxValue));
            }
        }

        // Any empty area is marked as border
        foreach (var areaSegment in AreaSegmentGraph.GetAllNodeData())
        {
            if (areaSegment.Type == AreaSegment.EAreaSegmentType.Empty)
                areaSegment.Type = AreaSegment.EAreaSegmentType.Border;
        }
    }

    // Create path lines
    private void CreatePathLines()
    {
        foreach (var edge in AreaSegmentGraph.GetAllEdges())
        {
            int edgeValue = AreaSegmentGraph.GetEdgeValue(edge);
            Vector2 leftCenter = _areaSegmentCenterMap[edge.x];
            Vector2 rightCenter = _areaSegmentCenterMap[edge.y];

            // Add path lines
            switch (edgeValue)
            {
                case (int)AreaSegment.EAreaSegmentEdgeType.MainPath:
                    MainPathLines.Add(new[] { leftCenter, rightCenter });
                    break;
                case (int)AreaSegment.EAreaSegmentEdgeType.SidePath:
                    SidePathLines.Add(new[] { leftCenter, rightCenter });
                    break;
            }
        }
    }

    // Create area segments borders
    private void CreateAreaBlockerLines()
    {
        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            int leftAreaSegmentID = _siteAreaSegmentMap[edge.LeftSite.Coord];
            int rightAreaSegmentID = _siteAreaSegmentMap[edge.RightSite.Coord];
            AreaSegment leftAreaSegment = AreaSegmentGraph.GetNodeData(leftAreaSegmentID);
            AreaSegment rightAreaSegment = AreaSegmentGraph.GetNodeData(rightAreaSegmentID);

            var leftNeighborhood = AreaSegmentGraph.GetNeighbours(leftAreaSegmentID);

            if (leftAreaSegmentID == rightAreaSegmentID ||
                leftAreaSegment.Type == AreaSegment.EAreaSegmentType.Border ||
                rightAreaSegment.Type == AreaSegment.EAreaSegmentType.Border ||
                !leftNeighborhood.Contains(rightAreaSegmentID) ||
                AreaSegmentGraph.GetEdgeValue(leftAreaSegmentID, rightAreaSegmentID) != (int)AreaSegment.EAreaSegmentEdgeType.NonNavigable)
                continue;

            var p0 = edge.ClippedEnds[LR.LEFT].ToUnityVector2();
            var p1 = edge.ClippedEnds[LR.RIGHT].ToUnityVector2();
            var segment = new[] { p0, p1 };
            AreaBlockerLines.Add(segment);
        }
    }

    // Create border blocker polygon
    private void CreateBorderBlockerPolygon(float borderInlandOffset)
    {
        var segments = new List<KeyValuePair<Edge, Vector2>>();

        foreach (var edge in VoronoiDiagram.Edges)
        {
            //Check if this edge is visible before continuing
            if (!edge.Visible()) continue;

            int leftAreaSegmentID = _siteAreaSegmentMap[edge.RightSite.Coord];
            int rightAreaSegmentID = _siteAreaSegmentMap[edge.LeftSite.Coord];
            AreaSegment leftAreaSegment = AreaSegmentGraph.GetNodeData(leftAreaSegmentID);
            AreaSegment rightAreaSegment = AreaSegmentGraph.GetNodeData(rightAreaSegmentID);

            // Either one or the other must be a border type
            if (leftAreaSegment.Type != rightAreaSegment.Type &&
                (leftAreaSegment.Type == AreaSegment.EAreaSegmentType.Border || rightAreaSegment.Type == AreaSegment.EAreaSegmentType.Border))
            {
                // Add border edge with the biome center to scale inwards later
                segments.Add(new KeyValuePair<Edge, Vector2>(edge,
                    leftAreaSegment.Type == AreaSegment.EAreaSegmentType.Border ? edge.LeftSite.Coord.ToUnityVector2() : edge.RightSite.Coord.ToUnityVector2()));
            }
        }

        // Group connected segments
        var edgeGroups = new List<List<KeyValuePair<Edge, Vector2>>>();
        while (segments.Count > 0)
        {
            var edges = new List<KeyValuePair<Edge, Vector2>>();
            var startEdge = segments[0];
            segments.Remove(startEdge);
            Vertex headPoint = startEdge.Key.RightVertex;
            Vertex tailPoint = startEdge.Key.LeftVertex;
            edges.Add(startEdge);

            // Find a polygon
            var polygonClosed = false;
            while (!polygonClosed && segments.Count > 0)
            {
                for (int i = 0; i < segments.Count; i++)
                {
                    var currentElement = segments[i];
                    Vertex leftPoint = currentElement.Key.LeftVertex;
                    Vertex rightPoint = currentElement.Key.RightVertex;
                    if (leftPoint == headPoint)
                    {
                        edges.Add(currentElement);
                        segments.Remove(currentElement);
                        headPoint = rightPoint;
                    }
                    else if (rightPoint == headPoint)
                    {
                        edges.Add(currentElement);
                        segments.Remove(currentElement);
                        headPoint = leftPoint;
                    }

                    // Polygon has been closed
                    if (headPoint == tailPoint)
                    {
                        polygonClosed = true;
                        break;
                    }
                }
            }
            edgeGroups.Add(edges);
        }

        // Iterate over each polygon found previously
        foreach (var edges in edgeGroups)
        {
            var polygon = new List<Vector2>();
            var coastLines = edges.Select(pair => pair.Key).ToList().EdgesToSortedLines();
            foreach (var line in coastLines)
            {

                // Offset borders towards biome center
                var left = line[0];
                var right = line[1];
                var center = Vector2.zero;
                edges.ForEach(e =>
                {
                    var l = e.Key.ClippedEnds[LR.LEFT].ToUnityVector2();
                    var r = e.Key.ClippedEnds[LR.RIGHT].ToUnityVector2();
                    if ((l == left || l == right) && (r == left || r == right))
                        center = e.Value;
                });

                left += (center - left).normalized * borderInlandOffset;
                right += (center - right).normalized * borderInlandOffset;

                // Offsetting can give duplicated points
                if (!polygon.Contains(left))
                    polygon.Add(left);
                if (!polygon.Contains(right))
                    polygon.Add(right);
            }

            // Create border blocker lines
            for (int j = 0; j < polygon.Count; j++)
            {
                var p0 = polygon[j];
                var p1 = j + 1 == polygon.Count ?
                    polygon[0] : polygon[j + 1];

                // Filter duplicated vertices -> TODO: fix problem in offsetting
                if ((p0 - p1).magnitude < 0.01f)
                    continue;

                BorderBlockerLines.Add(new[] { p0, p1 });
            }
        }
    }

    private void CreatePathPolygons(float roadWidth)
    {
        foreach (var line in MainPathLines)
        {
            var start = line[0];
            var end = line[1];

            var direction = (end - start).normalized;
            var normal = (Vector2)Vector3.Cross(direction, Vector3.forward).normalized;

            var p0 = start - direction * roadWidth + normal * roadWidth;
            var p1 = start - direction * roadWidth - normal * roadWidth;
            var p2 = end + direction * roadWidth + normal * roadWidth;
            var p3 = end + direction * roadWidth - normal * roadWidth;
            var origin = (p0 + p1 + p2 + p3) / 4;

            var poly = new List<Vector2> { p0, p1, p2, p3 };
            poly.SortVertices(origin);
            PathPolygons.Add(poly.ToArray());
        }

        foreach (var line in SidePathLines)
        {
            var start = line[0];
            var end = line[1];

            var direction = (end - start).normalized;
            var normal = (Vector2)Vector3.Cross(direction, Vector3.forward).normalized;

            var p0 = start - direction * roadWidth + normal * roadWidth;
            var p1 = start - direction * roadWidth - normal * roadWidth;
            var p2 = end + direction * roadWidth + normal * roadWidth;
            var p3 = end + direction * roadWidth - normal * roadWidth;
            var origin = (p0 + p1 + p2 + p3) / 4;

            var poly = new List<Vector2> { p0, p1, p2, p3 };
            poly.SortVertices(origin);
            PathPolygons.Add(poly.ToArray());
        }

    }

    //---------------------------------------------------------------
    //
    //              HELPER FUNCTIONS
    //
    //---------------------------------------------------------------

    // Checks if segments are neighbours
    private bool AreNeighbours(IEnumerable<int> areaSegments)
    {
        // Check for null
        if (areaSegments == null || !areaSegments.Any())
            return false;

        // Check if segments are neighbours
        var found = new List<int> { };
        var frontier = new Queue<int>();
        frontier.Enqueue(areaSegments.ElementAt(0));
        while (found.Count < areaSegments.Count() && frontier.Count > 0)
        {
            var element = frontier.Dequeue();
            if (areaSegments.Contains(element))
            {
                found.Add(element);
                foreach (int neighbour in AreaSegmentGraph.GetNeighbours(element))
                {
                    frontier.Enqueue(neighbour);
                }
            }
        }
        return found.Count == areaSegments.Count();
    }

    // Generates a polygon for a given voronoi site
    private Vector2[] GetSitePolygon(Vector2f site)
    {
        List<Edge> edges = new List<Edge>();
        foreach (var edge in VoronoiDiagram.Edges)
        {
            if (!edge.Visible()) continue;

            if (edge.LeftSite.Coord != site && edge.RightSite.Coord != site)
                continue;

            edges.Add(edge);
        }
        return edges.EdgesToSortedLines().Select(t => t[0]).ToArray();
    }

    // Get closest AreaSegment center to specified pos
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