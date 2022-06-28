using System.Collections.Generic;
using UnityEngine;

public class PathQuadratic
{
    [SerializeField, HideInInspector] private List<Vector3> points;

    public PathQuadratic(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.left,
            center + Vector3.forward,
            center + Vector3.right
        };
    }

    public Vector3 this[int i] => points[i];

    public int NumPoints => points.Count;
    
    public int NumSegments => points.Count / 2;


    public void AddSegment(Vector3 anchorPos)
    {
        points.Add(points[NumPoints - 1] + Vector3.forward);
        points.Add(anchorPos);
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new[] { points[i * 2], points[i * 2 + 1], points[i * 2 + 2]};
    }

    public void MovePoint(int i, Vector3 pos)
    {
        points[i] = pos;
    }
}