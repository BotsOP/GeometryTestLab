using System.Collections.Generic;
using UnityEngine;

public class Path
{
    [SerializeField, HideInInspector] private List<Vector3> points;

    public Path(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.left,
            center + (Vector3.left + Vector3.forward) * 0.5f,
            center + (Vector3.right + Vector3.forward) * 0.5f,
            center + Vector3.right
        };
    }
    public Path()
    {
        
    }
    public void SetList(List<Vector3> newPoints)
    {
        points = newPoints;
    }

    public Vector3 this[int i] => points[i];

    public int NumPoints => points.Count;
    
    public int NumSegments => (points.Count - 4) / 3 + 1;


    public void AddSegment(Vector3 anchorPos)
    {
        points.Add(points[NumPoints - 1]* 2 - points[NumPoints - 2]);
        points.Add((points[NumPoints - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }

    public void MovePoint(int i, Vector3 pos)
    {
        points[i] = pos;
    }
}
