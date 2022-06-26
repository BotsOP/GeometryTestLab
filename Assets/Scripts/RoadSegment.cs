using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadSegment : MonoBehaviour
{
    [SerializeField] private Mesh2D shape2D;
    [Range(2,32)]
    [SerializeField] int segmentCount = 8;
    
    [Range(0, 1)] [SerializeField] private float tTest;
    [SerializeField] private Transform[] controlPoints = new Transform[4];

    private Mesh mesh;

    Vector3 GetPos(int i) => controlPoints[i].position;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "roagSegment";
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void Update()
    {
        GenerateMesh();
    }


    void GenerateMesh()
    {
        mesh.Clear();

        float uSpan = shape2D.CalcUSpan();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        
        float[] fArr = new float[32];
        CalcLengthTableInto(fArr);
        
        for (int ring = 0; ring < segmentCount; ring++)
        {
            float t = ring / (segmentCount - 1f);
            OrientedPoint op = GetBezierPoint(t); 
            
            for (int i = 0; i < shape2D.VertexCount; i++)
            {
                verts.Add(op.LocalToWorldPosition(shape2D.vertices[i].point));
                normals.Add(op.LocalToWorldVect(shape2D.vertices[i].normal));
                //uvs.Add(new Vector2(shape2D.vertices[i].u, t * GetApproxLength() / uSpan));
                uvs.Add(new Vector2(shape2D.vertices[i].u,  FloatArrayExtensions.Sample(fArr, t) / uSpan ));
            }
        }

        List<int> triIndices = new List<int>();
        for (int ring = 0; ring < segmentCount - 1; ring++)
        {
            int rootIndex = ring * shape2D.VertexCount;
            int rootIndexNext = (ring + 1) * shape2D.VertexCount;

            for (int line = 0; line < shape2D.LineCount; line+=2)
            {
                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line + 1];
                
                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;
                int nextA = rootIndexNext + lineIndexA;
                int nextB = rootIndexNext + lineIndexB;
                
                triIndices.Add(currentA);
                triIndices.Add(nextA);
                triIndices.Add(nextB);
                
                triIndices.Add(currentA);
                triIndices.Add(nextB);
                triIndices.Add(currentB);
            }
        }
        
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triIndices, 0);
        
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawSphere(GetPos(i), 0.05f);
        }

        Handles.DrawBezier(
            GetPos(0),
            GetPos(3),
            GetPos(1),
            GetPos(2), Color.white, EditorGUIUtility.whiteTexture, 1f
        );

        OrientedPoint point = GetBezierPoint(tTest);
        
        float radius = 0.03f;

        Vector3[] localVerts = shape2D.vertices.Select(v => point.LocalToWorldPosition(v.point)).ToArray();

        for (int i = 0; i < shape2D.lineIndices.Length; i+=2)
        {
            Vector3 a = localVerts[shape2D.lineIndices[i]];
            Vector3 b = localVerts[shape2D.lineIndices[i + 1]];
            
            Gizmos.DrawLine(a,b);
        }
    }

    OrientedPoint GetBezierPoint(float t)
    {
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;
        
        return new OrientedPoint(pos, tangent);
    }

    float GetApproxLength(int precision = 16)
    {
        Vector3[] points = new Vector3[precision];
        for (int i = 0; i < precision; i++)
        {
            float t = i / (precision - 1);
            points[i] = GetBezierPoint(t).pos;
        }

        float dist = 0;
        for (int i = 0; i < precision - 1; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[i + 1];
            dist += Vector3.Distance(a, b);
        }

        return dist;
    }

    private void CalcLengthTableInto(float[] arr)
    {
        arr[0] = 0f;
        float totalLength = 0f;
        Vector3 prev = GetPos(0);

        for (int i = 0; i < arr.Length; i++)
        {
            float t = ((float)i) / (arr.Length - 1);
            Vector3 pt = GetBezierPoint(t).pos;
            float diff = (prev - pt).magnitude;
            totalLength += diff;
            arr[i] = totalLength;
            prev = pt;
        }
    }
}

public struct OrientedPoint
    {
        public Vector3 pos;
        public Quaternion rot;

        public OrientedPoint(Vector3 pos, Quaternion rot)
        {
            this.pos = pos;
            this.rot = rot;
        }
        
        public OrientedPoint(Vector3 pos, Vector3 forward)
        {
            this.pos = pos;
            rot = Quaternion.LookRotation(forward);
        }

        public Vector3 LocalToWorldPosition(Vector3 localPos)
        {
            return pos + rot * localPos;
        }
        
        public Vector3 LocalToWorldVect(Vector3 localPos)
        {
            return rot * localPos;
        }
    }

public static class FloatArrayExtensions
{
    public static float Sample(this float[] fArr, float t)
    {
        float count = fArr.Length;
        if (count == 0)
        {
            Debug.LogError("Unable to sample array - it has no elements");
            return 0;
        }

        if (count == 1)
        {
            return fArr[0];
        }

        float iFloat = t * (count * 1f - 1f);
        
        int idLower = Mathf.FloorToInt(iFloat);
        int idUpper = Mathf.FloorToInt(iFloat + 1);
        
        if (idUpper >= count)
        {
            return fArr[(int)count - 1];
        }

        if (idLower < 0)
        {
            return fArr[0];
        }
        //Debug.Log(fArr[idLower] + "   " + fArr[idUpper] + "   " + (iFloat - idLower) + "       " + Mathf.Lerp(fArr[idLower], fArr[idUpper], (iFloat - idLower)));
        return Mathf.Lerp(fArr[idLower], fArr[idUpper], iFloat - idLower);

    }
}


