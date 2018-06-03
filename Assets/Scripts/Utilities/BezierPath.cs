using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BezierPath
{

    [SerializeField]
    private readonly List<Vector2> _points = new List<Vector2>();
    [SerializeField]
    private readonly bool _autoSetControlPoints = true;

    public BezierPath(List<Vector2> controlPoints)
    {
        var first = controlPoints[0];
        var second = controlPoints[1];

        controlPoints.Remove(first);
        controlPoints.Remove(second);
        
        var startControl = (second - first).normalized * 2;
        _points.Add(first);
        _points.Add(first + startControl);
        _points.Add(second - startControl);
        _points.Add(second);

        foreach (var point in controlPoints)
        {
            AddSegment(point);
        }
    }

    public Vector2 this[int i]
    {
        get
        {
            return _points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return _points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return _points.Count / 3;
        }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        _points.Add(_points[_points.Count - 1] * 2 - _points[_points.Count - 2]);
        _points.Add((_points[_points.Count - 1] + anchorPos) * .5f);
        _points.Add(anchorPos);

        if (_autoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(_points.Count - 1);
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new[] { _points[i * 3], _points[i * 3 + 1], _points[i * 3 + 2], _points[LoopIndex(i * 3 + 3)] };
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2> { _points[0] };
        Vector2 previousPoint = _points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments ; segmentIndex++)
        {
            Vector2[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }


    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < _points.Count)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }

        AutoSetStartAndEndControls();
    }


    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = _points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0)
        {
            Vector2 offset = _points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0)
        {
            Vector2 offset = _points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < _points.Count)
            {
                _points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
            }
        }
    }

    void AutoSetStartAndEndControls()
    {
        _points[1] = (_points[0] + _points[2]) * .5f;
        _points[_points.Count - 2] = (_points[_points.Count - 1] + _points[_points.Count - 3]) * .5f;
    }

    int LoopIndex(int i)
    {
        return (i + _points.Count) % _points.Count;
    }

}