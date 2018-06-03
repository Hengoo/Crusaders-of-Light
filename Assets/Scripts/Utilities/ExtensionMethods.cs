using System;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ExtensionMethods
{
    public static Dictionary<TKey, TValue> DeepClone<TKey, TValue>
        (Dictionary<TKey, TValue> original) where TValue : ICloneable
    {
        Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
            original.Comparer);
        foreach (KeyValuePair<TKey, TValue> entry in original)
        {
            ret.Add(entry.Key, (TValue)entry.Value.Clone());
        }
        return ret;
    }

    public static Vector3 Barycentric(this Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 v0 = b - a, v1 = c - a, v2 = p - a;
        float d00 = Vector2.Dot(v0, v0);
        float d01 = Vector2.Dot(v0, v1);
        float d11 = Vector2.Dot(v1, v1);
        float d20 = Vector2.Dot(v2, v0);
        float d21 = Vector2.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1.0f - v - w;

        return new Vector3(u, v, w);
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    // Rotates objects if outside of angle tolerance
    public static void CorrectAngleTolerance(this GameObject go, float angleLimit)
    {
        float angle = Vector3.Angle(go.transform.up, Vector3.up);
        if (angle > angleLimit)
        {
            var euler = go.transform.rotation.eulerAngles;
            go.transform.rotation = Quaternion.RotateTowards(go.transform.rotation, Quaternion.Euler(0, euler.y, 0), angle - angleLimit);
        }
    }

    // Supports both convex and non convex polygons
    public static bool IsInsidePolygon(this Vector2 p, Vector2[] polyPoints)
    {
        var j = polyPoints.Length - 1;
        var inside = false;
        for (var i = 0; i < polyPoints.Length; j = i++)
        {
            if (((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) ||
                (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) &&
                (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x))
                inside = !inside;
        }
        return inside;
    }

    public static void SortVertices(this List<Vector2> polygon, Vector2 origin)
    {
        polygon.Sort(new ClockwiseComparer(origin));
    }

    public static List<Vector2> EdgesToPolygon(this List<Edge> edges)
    {
        var result = new List<Vector2>();
        var edgeReorderer = new EdgeReorderer(edges, typeof(Vertex));
        for (var j = 0; j < edgeReorderer.Edges.Count; j++)
        {
            var edge = edgeReorderer.Edges[j];
            if (!edge.Visible()) continue;

            result.Add(edge.ClippedEnds[edgeReorderer.EdgeOrientations[j]].ToUnityVector2());
        }

        return result;
    }

    public static List<Vector2[]> EdgesToSortedLines(this List<Edge> edges)
    {
        var result = new List<Vector2[]>();
        var edgeReorderer = new EdgeReorderer(edges, typeof(Vertex));
        for (var j = 0; j < edgeReorderer.Edges.Count; j++)
        {
            var edge = edgeReorderer.Edges[j];
            if (!edge.Visible()) continue;

            var p0 = edge.ClippedEnds[edgeReorderer.EdgeOrientations[j]].ToUnityVector2();
            var p1 = edge.ClippedEnds[edgeReorderer.EdgeOrientations[j] == LR.LEFT ? LR.RIGHT : LR.LEFT].ToUnityVector2();
            result.Add(new[] { p0, p1 });
        }

        return result;
    }

    /// <summary>
    ///     ClockwiseComparer provides functionality for sorting a collection of Vector2s such
    ///     that they are ordered clockwise about a given origin.
    /// </summary>
    private class ClockwiseComparer : IComparer<Vector2>
    {
        private Vector2 m_Origin;

        #region Properties

        /// <summary>
        ///     Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the ClockwiseComparer class.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparer(Vector2 origin)
        {
            m_Origin = origin;
        }

        #region IComparer Methods

        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        public int Compare(Vector2 first, Vector2 second)
        {
            return IsClockwise(first, second, m_Origin);
        }

        #endregion

        /// <summary>
        ///     Returns 1 if first comes before second in clockwise order.
        ///     Returns -1 if second comes before first.
        ///     Returns 0 if the points are identical.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        /// <param name="origin">Origin.</param>
        public static int IsClockwise(Vector2 first, Vector2 second, Vector2 origin)
        {
            if (first == second)
                return 0;

            Vector2 firstOffset = first - origin;
            Vector2 secondOffset = second - origin;

            float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.y);
            float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.y);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1;
        }
    }
}
