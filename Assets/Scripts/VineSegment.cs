using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VineSegment : MonoBehaviour
{
    [Range(4, 32)]
    [SerializeField] private int roundSegments = 16;

    [Range(2, 64)] 
    [SerializeField] private int curveSegments = 16;

    [Range(0, 1)] [SerializeField] private float vineDiameter = 1f;

    [SerializeField] private AnimationCurve vineSizeCurve;

    [HideInInspector] public Path path;
    [SerializeField] private List<Transform> transformPoints;

    [SerializeField] private GameObject emptyObject;
    public VineGenerator VineGenerator;
    private Mesh mesh;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void GenerateMesh()
    {
        path = new Path(transform.position);
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < VineGenerator.points.Count; i++)
        {
            points.Add(VineGenerator.points[i].pos);
        }
        path.SetList(points);
        
        
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triIndices = new List<int>();
        
        for (int i = 0; i < path.NumSegments; i++)
        {
            
            float[] fArr = new float[16];
            CalcLengthTableInto(fArr, i);
            
            for (int segment = 0; segment < curveSegments; segment++)
            {
                float t = segment / (curveSegments - 1f);
                
                float vineSize = (float)i / (path.NumSegments - 1);
                float vineSizeNext = (float)(i + 1) / (path.NumSegments - 1);
                vineSize = Mathf.Lerp(vineSize, vineSizeNext, t);
                vineSize = vineSizeCurve.Evaluate(vineSize);
                
                
                Vector3 up = Vector3.Lerp(VineGenerator.points[i * 3].rot * Vector3.up, VineGenerator.points[(i + 1) * 3].rot * Vector3.up, t);
                OrientedPoint op = GetBezierPoint(t, i, up);
                //Debug.DrawRay(op.pos, op.rot * Vector3.up * 0.1f);
                
                for (int vertex = 0; vertex < roundSegments + 1; vertex++)
                {
                    float u = (float)vertex / roundSegments;
                    float amountDegrees = 360 / roundSegments * (vertex % roundSegments);
                    Vector3 dir = op.LocalToWorldVect(Quaternion.Euler(0, 0, amountDegrees) * Vector3.right * (vineDiameter * vineSize));
                    Vector3 newPos = dir + op.pos;
                    verts.Add(newPos);
                    normals.Add(dir);
                    uvs.Add(new Vector2(u,  fArr.Sample(t)));
                }
            }
            
            for (int segment = 0; segment < curveSegments - 1; segment++)
            {
                int rootIndex = (roundSegments + 1) * (segment + curveSegments * i);
                int rootIndexNext = (roundSegments + 1) * (1 + segment + curveSegments * i);
                
                for (int j = 0; j < roundSegments; j++)
                {
                    int currentA = rootIndex + j;
                    int currentB = (rootIndex + (j + 1) % (roundSegments + 1));
                    int nextA = rootIndexNext + j;
                    int nextB = (rootIndexNext + (j + 1) % (roundSegments + 1));
                    
                    triIndices.Add(nextA);
                    triIndices.Add(currentA);
                    triIndices.Add(currentB);
                    
                    triIndices.Add(nextB);
                    triIndices.Add(nextA);
                    triIndices.Add(currentB);
                }
            }
        }
        
        mesh.SetVertices(verts);
        mesh.SetTriangles(triIndices, 0);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
    }

    private void Update() => GenerateMesh();
    
    private void CalcLengthTableInto(float[] arr, int segment)
    {
        arr[0] = 0f;
        float totalLength = 0f;
        Vector3 prev = path.GetPointsInSegment(segment)[0];

        for (int i = 0; i < arr.Length; i++)
        {
            float t = ((float)i) / (arr.Length - 1);
            Vector3 pt = GetBezierPoint(t, segment).pos;
            float diff = (prev - pt).magnitude;
            totalLength += diff;
            arr[i] = totalLength;
            prev = pt;
        }
    }

    // private void OnDrawGizmos()
    // {
    //     path = new Path(transform.position);
    //     path.AddSegment(Vector3.right * 10);
    //
    //     int transformPointCount = transformPoints.Count;
    //     
    //     if (path.NumPoints > transformPointCount)
    //     {
    //         Debug.Log("not enough points  " + path.NumPoints + "  " + transformPoints.Count);
    //         for (int i = 0; i < path.NumPoints - transformPointCount; i++)
    //         {
    //             GameObject newObject =
    //                 Instantiate(emptyObject, path[transformPointCount + i], Quaternion.identity, transform);
    //             
    //             var iconContent = EditorGUIUtility.IconContent("Assets/Textures/emptyImage.png");
    //             EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);
    //             
    //             transformPoints.Add(newObject.transform);
    //         }
    //     }
    //     
    //     else if (path.NumPoints < transformPoints.Count)
    //     {
    //         Debug.Log("too many points  " + path.NumPoints + "  " + transformPoints.Count);
    //         for (int i = 0; i < transformPoints.Count - path.NumPoints; i++)
    //         {
    //             DestroyImmediate(transformPoints[transformPoints.Count - path.NumPoints + i]);
    //             transformPoints.Remove(transformPoints[transformPoints.Count - path.NumPoints + i]);
    //         }
    //     }
    //
    //     for (int i = 0; i < path.NumPoints; i++)
    //     {
    //         path.MovePoint(i, transformPoints[i].position);
    //     }
    //
    //     for (int i = 0; i < path.NumSegments; i++)
    //     {
    //         int startIndex = i * 3;
    //         Handles.DrawBezier(path[startIndex], 
    //             path[startIndex + 3], 
    //             path[startIndex + 1], 
    //             path[startIndex + 2], Color.white, EditorGUIUtility.whiteTexture, 1f);
    //
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawLine(path[startIndex], path[startIndex + 1]);
    //         Gizmos.DrawLine(path[startIndex + 2], path[startIndex + 3]);
    //         Gizmos.color = Color.white;
    //     }
    //
    //     for (int i = 0; i < path.NumPoints; i++)
    //     {
    //         if (i % 3 != 0)
    //         {
    //             Gizmos.color = Color.red;
    //         }
    //         Gizmos.DrawSphere(path[i], 0.05f);
    //         Gizmos.color = Color.white;
    //     }
    //
    //     Gizmos.color = Color.blue;
    //
    //     // OrientedPoint op = GetBezierPoint(tTest, 0);
    //     //
    //     // Gizmos.DrawSphere(op.pos, 0.05f);
    //     //
    //     // for (int vertex = 0; vertex < roundSegments; vertex++)
    //     // {
    //     //     float amountDegrees = 360 / roundSegments * vertex;
    //     //     Vector3 newPos = op.LocalToWorldVect(Quaternion.Euler(0, 0, amountDegrees) * Vector3.right) + op.pos;
    //     //     Gizmos.DrawSphere(newPos, 0.05f);
    //     // }
    //     
    //     // List<Vector3> verts = new List<Vector3>();
    //     // for (int segment = 0; segment < curveSegments; segment++)
    //     // {
    //     //     float t = segment / (curveSegments - 1f);
    //     //     OrientedPoint op = GetBezierPoint(t, 0); 
    //     //         
    //     //     for (int vertex = 0; vertex < roundSegments; vertex++)
    //     //     {
    //     //         float amountDegrees = 360 / roundSegments * vertex;
    //     //         Vector3 newPos = op.LocalToWorldVect(Quaternion.Euler(0, 0, amountDegrees) * Vector3.right) + op.pos;
    //     //         verts.Add(newPos);
    //     //
    //     //         Gizmos.DrawSphere(newPos, 0.05f);
    //     //         
    //     //         Gizmos.color = Color.blue;
    //     //     }
    //     // }
    //     //
    //     // Gizmos.DrawSphere(verts[3], 0.09f);
    //     //
    //     // for (int segment = 0; segment < curveSegments - 1; segment++)
    //     // {
    //     //     int rootIndex = roundSegments * segment;
    //     //     int rootIndexNext = (roundSegments) * (segment + 1);
    //     //     
    //     //     //Debug.Log(rootIndex + "   " + rootIndexNext);
    //     //         
    //     //     for (int j = 0; j < roundSegments; j++)
    //     //     {
    //     //         int currentA = rootIndex + j;
    //     //         int currentB = (rootIndex + (j + 1) % roundSegments);
    //     //         int nextA = rootIndexNext + j;
    //     //         int nextB = (rootIndexNext + (j + 1) % roundSegments);
    //     //         
    //     //         Debug.Log(currentB + "    " + currentA + "    " + nextA + "  tri1  " + rootIndexNext);
    //     //         Debug.Log(currentB + "    " + nextA + "    " + nextB + "  tri2  ");
    //     //         
    //     //         Gizmos.color = Color.green;
    //     //             
    //     //         Gizmos.DrawLine(verts[currentB], verts[currentA]);
    //     //         Gizmos.DrawLine(verts[currentA], verts[nextA]);
    //     //         Gizmos.DrawLine(verts[nextA], verts[currentB]);
    //     //         
    //     //         Gizmos.color = Color.yellow;
    //     //         
    //     //         Gizmos.DrawLine(verts[currentB], verts[nextA]);
    //     //         Gizmos.DrawLine(verts[nextA], verts[nextB]);
    //     //         Gizmos.DrawLine(verts[nextB], verts[currentB]);
    //     //         
    //     //         Gizmos.color = Color.white;
    //     //     }
    //     // }
    //     
    //     Gizmos.color = Color.white;
    //     
    //     Event guiEvent = Event.current;
    //
    //     if (guiEvent.button == 2 && guiEvent.isMouse)
    //     {
    //         while (transform.childCount > 0)
    //         {
    //             foreach (Transform child in transform) {
    //                 DestroyImmediate(child.gameObject);
    //             }
    //         }
    //         transformPoints.Clear();
    //         path = new Path(transform.position);
    //     }
    //     // if (guiEvent.button == 0 && guiEvent.shift)
    //     // {
    //     //     Vector3 anchorPos = GetBezierPoint(1, path.NumSegments).rot * Vector3.forward + path[path.NumPoints - 1];
    //     //     path.AddSegment(anchorPos);
    //     // }
    //     
    // }
    
    OrientedPoint GetBezierPoint(float t, int segment, Vector3 up)
    {
        Vector3[] points = path.GetPointsInSegment(segment);
        Vector3 p0 = points[0];
        Vector3 p1 = points[1];
        Vector3 p2 = points[2];
        Vector3 p3 = points[3];

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;

        return new OrientedPoint(pos, tangent, up);
    }
    
    OrientedPoint GetBezierPoint(float t, int segment)
    {
        Vector3[] points = path.GetPointsInSegment(segment);
        Vector3 p0 = points[0];
        Vector3 p1 = points[1];
        Vector3 p2 = points[2];
        Vector3 p3 = points[3];

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;

        return new OrientedPoint(pos, tangent);
    }
}
