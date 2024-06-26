﻿using System;
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

    // Get rotation of the terrain normal
    public static Quaternion GetNormalRotation(this Terrain terrain, Vector3 position)
    {
        Ray ray = new Ray(position + Vector3.up * 30, Vector3.down);
        var hits = Physics.RaycastAll(ray, Mathf.Infinity);
        var terrainHit = hits.First(hit => hit.collider.name == terrain.name);
        return Quaternion.FromToRotation(Vector3.up, terrainHit.normal);
    }

    public static Vector2[] Get2DPolygon(this BoxCollider box)
    {
        Vector2 p0 = new Vector2(box.transform.localScale.x * (box.center.x + box.size.x / 2f), box.transform.localScale.z * (box.center.z + box.size.z / 2f));
        Vector2 p1 = new Vector2(box.transform.localScale.x * (box.center.x + box.size.x / 2f), box.transform.localScale.z * (box.center.z - box.size.z / 2f));
        Vector2 p2 = new Vector2(box.transform.localScale.x * (box.center.x - box.size.x / 2f), box.transform.localScale.z * (box.center.z - box.size.z / 2f));
        Vector2 p3 = new Vector2(box.transform.localScale.x * (box.center.x - box.size.x / 2f), box.transform.localScale.z * (box.center.z + box.size.z / 2f));

        return new[] { p0, p1, p2, p3 };
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
    public static bool IsInsidePolygon(this Vector2 p, IEnumerable<Vector2> polyPoints)
    {
        var j = polyPoints.Count() - 1;
        var inside = false;
        for (var i = 0; i < polyPoints.Count(); j = i++)
        {
            if (((polyPoints.ElementAt(i).y <= p.y && p.y < polyPoints.ElementAt(j).y) ||
                 (polyPoints.ElementAt(j).y <= p.y && p.y < polyPoints.ElementAt(i).y)) &&
                (p.x < (polyPoints.ElementAt(j).x - polyPoints.ElementAt(i).x) * (p.y - polyPoints.ElementAt(i).y) / (polyPoints.ElementAt(j).y - polyPoints.ElementAt(i).y) + polyPoints.ElementAt(i).x))
                inside = !inside;
        }
        return inside;
    }

    public static Vector2 GetPolygonCenter(this IEnumerable<Vector2> polyPoints)
    {
        var count = polyPoints.Count();
        var center = Vector2.zero;
        foreach (var p in polyPoints)
        {
            center += p;
        }
        return center / count;
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

    public static IEnumerable<Vector2> OffsetToCenter(this IEnumerable<Vector2> polygon, Vector2 center, float amount, List<int> skip = null)
    {
        var list = polygon.ToList();
        for(var i = 0; i < list.Count; i++)
        {
            if(skip != null && skip.Contains(i))
                continue;

            list[i] += (center - list[i]).normalized * amount;
        }

        return list;
    }

    public static Vector2 ClosestPoint(this Vector2[] polygon, Vector2 point)
    {
        Vector2 closestPoint = Vector2.zero;
        float closestDistance = float.MaxValue;

        foreach (var p in polygon)
        {
            float currentDistance = (p - point).sqrMagnitude;
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestPoint = p;
            }
        }

        return closestPoint;
    }

    public static List<Vector2[]> PolygonToLines(this IEnumerable<Vector2> reference, List<int> skip = null)
    {
        var result = new List<Vector2[]>();

        // Create border blocker lines
        for (int j = 0; j < reference.Count(); j++)
        {
            var p0 = reference.ElementAt(j);
            int next = j + 1 == reference.Count() ? 0 : j + 1;
            if (skip != null && skip.Contains(j) && skip.Contains(next))
                continue;

            var p1 = reference.ElementAt(next);

            // Filter any duplicated vertices
            if ((p0 - p1).magnitude < 0.01f)
                continue;

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
