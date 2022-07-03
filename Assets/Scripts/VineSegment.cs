using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VineSegment : MonoBehaviour
{
    [Range(4, 32)]
    [SerializeField] private int roundSegments = 16;

    [Range(2, 64)] 
    [SerializeField] private int curveSegments = 16;

    [Range(0, 0.5f)] [SerializeField] private float vineDiameter = 1f;
    [Range(0.01f, 0.5f)] [SerializeField] private float leafSize = 1f;

    [SerializeField] private AnimationCurve vineSizeCurve;
    
    private Path path;
    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        mesh.name = "vines";
    }

    public void GenerateMesh(List<OrientedPoint> points)
    {
        path = new Path();
        path.SetList(points.Select(point => point.pos).ToList());
        
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
                
                Vector3 up = Vector3.Lerp(points[i * 3].rot * Vector3.up, points[(i + 1) * 3].rot * Vector3.up, t);
                OrientedPoint op = GetBezierPoint(t, i, up);
                
                Debug.DrawRay(op.pos, op.rot * Vector3.forward * 0.1f, Color.blue);
                Debug.DrawRay(op.pos, op.rot * Vector3.up * 0.1f, Color.green);
                Debug.DrawRay(op.pos, op.rot * Vector3.right * 0.1f, Color.red);
                
                //Vine vertex placement
                for (int vertex = 0; vertex < roundSegments + 1; vertex++)
                {
                    float u = (float)vertex / roundSegments;
                    float amountDegrees = 360 / roundSegments * (vertex % roundSegments);
                    Vector3 dir = op.LocalToWorldVect(Quaternion.Euler(0, 0, amountDegrees) * Vector3.right * (vineDiameter * vineSize));
                    Vector3 newPos = dir + op.pos;
                    
                    //Remove transform changes
                    newPos = removeTransform(newPos);
                    dir = Quaternion.Inverse(transform.rotation) * dir;

                    verts.Add(newPos + Vector3.one * 0.001f);
                    normals.Add(dir);
                    uvs.Add(new Vector2(u,  fArr.Sample(t)));
                }
            }
            
            //Triangle linking
            for (int segment = 0; segment < curveSegments - 1; segment++)
            {
                int rootIndex = (roundSegments + 1) * (segment + curveSegments * i);
                int rootIndexNext = (roundSegments + 1) * (1 + segment + curveSegments * i);
                
                for (int j = 0; j < roundSegments; j++)
                {
                    int currentA = rootIndex + j;
                    int currentB = rootIndex + (j + 1) % (roundSegments + 1);
                    int nextA = rootIndexNext + j;
                    int nextB = rootIndexNext + (j + 1) % (roundSegments + 1);
                    
                    triIndices.Add(nextA);
                    triIndices.Add(currentA);
                    triIndices.Add(currentB);
                    
                    triIndices.Add(nextB);
                    triIndices.Add(nextA);
                    triIndices.Add(currentB);
                }
            }
        }
        
        //Leaf vine placement
        List<int> leafTriangles = new List<int>();
        for (int i = 0; i < path.NumSegments - 1; i++)
        {
            for (int j = -1; j < 2; j += 2)
            {
                int amountVineVertice = verts.Count;

                Vector3 upLeaf = Vector3.Lerp(points[i * 3].rot * Vector3.up, points[(i + 1) * 3].rot * Vector3.up, 0);
                Vector3 upLeafNext = Vector3.Lerp(points[i * 3].rot * Vector3.up, points[(i + 1) * 3].rot * Vector3.up, 0.5f);
                OrientedPoint opLeaf = GetBezierPoint(0, i, upLeaf);
                OrientedPoint opLeafNext = GetBezierPoint(0.5f, i, upLeafNext);

                float vineSize = vineSizeCurve.Evaluate((float)i / (path.NumSegments - 1));
                float vineSizeNext = vineSizeCurve.Evaluate((float)(i + 1) / (path.NumSegments - 1));

                Vector3 vineOffset = opLeaf.LocalToWorldPosition(Vector3.up * (vineDiameter * vineSize));
                Vector3 vineOffsetNext = opLeafNext.LocalToWorldPosition(Vector3.up * (vineDiameter * vineSizeNext));

                float x = Vector3.Distance(vineOffset, vineOffsetNext);
                
                Vector3 vineOffsetRight = opLeafNext.LocalToWorldPosition(new Vector3(j * x, 0, 0) + Vector3.up * (vineDiameter * vineSizeNext));
                Vector3 vineOffsetNextRight = opLeaf.LocalToWorldPosition(new Vector3(j * x, 0, 0) + Vector3.up * (vineDiameter * vineSize));
                
                verts.Add(removeTransform(vineOffset));
                verts.Add(removeTransform(vineOffsetNext));
                verts.Add(removeTransform(vineOffsetRight));
                verts.Add(removeTransform(vineOffsetNextRight));
            
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 1));
                uvs.Add(new Vector2(1, 1));
            
                for (int k = 0; k < 4; k++)
                {
                    normals.Add(opLeaf.rot * Vector3.up);
                }

                if (j == -1)
                {
                    leafTriangles.Add(amountVineVertice + 2);
                    leafTriangles.Add(amountVineVertice + 1);
                    leafTriangles.Add(amountVineVertice);
        
                    leafTriangles.Add(amountVineVertice);
                    leafTriangles.Add(amountVineVertice + 3);
                    leafTriangles.Add(amountVineVertice + 2);
                    continue;
                }
                leafTriangles.Add(amountVineVertice);
                leafTriangles.Add(amountVineVertice + 1);
                leafTriangles.Add(amountVineVertice + 2);
        
                leafTriangles.Add(amountVineVertice + 2);
                leafTriangles.Add(amountVineVertice + 3);
                leafTriangles.Add(amountVineVertice);
            }
        }

        mesh.subMeshCount = 2;
        
        mesh.SetVertices(verts);
        mesh.SetTriangles(triIndices, 0);
        mesh.SetTriangles(leafTriangles, 1);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        
        mesh.RecalculateBounds();
    }
    
    public void GenerateMesh(List<OrientedPoint>[] pointsArray)
    {
        path = new Path();
        
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triIndices = new List<int>();
        List<int> leafTriangles = new List<int>();

        int amountVineVertices = 0;
        
        for (int vineIndex = 0; vineIndex < pointsArray.Length; vineIndex++)
        {
            List<OrientedPoint> points = pointsArray[vineIndex];
            path.SetList(points.Select(point => point.pos).ToList());

            for (int vineSegment = 0; vineSegment < path.NumSegments; vineSegment++)
            {
                float[] fArr = new float[16];
                CalcLengthTableInto(fArr, vineSegment);
                
                for (int curveSegment = 0; curveSegment < curveSegments; curveSegment++)
                {
                    float t = curveSegment / (curveSegments - 1f);
                    
                    float vineSize = (float)vineSegment / (path.NumSegments - 1);
                    float vineSizeNext = (float)(vineSegment + 1) / (path.NumSegments - 1);
                    vineSize = Mathf.Lerp(vineSize, vineSizeNext, t);
                    vineSize = vineSizeCurve.Evaluate(vineSize);
                    
                    Vector3 up = Vector3.Lerp(points[vineSegment * 3].rot * Vector3.up, points[(vineSegment + 1) * 3].rot * Vector3.up, t);
                    OrientedPoint op = GetBezierPoint(t, vineSegment, up);
                    
                    Debug.DrawRay(op.pos, op.rot * Vector3.forward * 0.1f, Color.blue);
                    Debug.DrawRay(op.pos, op.rot * Vector3.up * 0.1f, Color.green);
                    Debug.DrawRay(op.pos, op.rot * Vector3.right * 0.1f, Color.red);
                    
                    //Vine vertex placement
                    for (int vertex = 0; vertex < roundSegments + 1; vertex++)
                    {
                        float u = (float)vertex / roundSegments;
                        float amountDegrees = 360 / roundSegments * (vertex % roundSegments);
                        Vector3 dir = op.LocalToWorldVect(Quaternion.Euler(0, 0, amountDegrees) * Vector3.right * (vineDiameter * vineSize));
                        Vector3 newPos = dir + op.pos;
                        
                        //Remove transform changes
                        newPos = removeTransform(newPos);
                        dir = Quaternion.Inverse(transform.rotation) * dir;

                        verts.Add(newPos + Vector3.one * 0.001f);
                        normals.Add(dir);
                        uvs.Add(new Vector2(u,  fArr.Sample(t)));
                    }
                }
                
                //Triangle linking
                for (int vineCurveSegments = 0; vineCurveSegments < curveSegments - 1; vineCurveSegments++)
                {
                    int rootIndex = ((roundSegments + 1) * (vineCurveSegments + curveSegments * vineSegment)) + amountVineVertices;
                    int rootIndexNext = ((roundSegments + 1) * (1 + vineCurveSegments + curveSegments * vineSegment)) + amountVineVertices;
                    
                    for (int j = 0; j < roundSegments; j++)
                    {
                        int currentA = rootIndex + j;
                        int currentB = rootIndex + (j + 1) % (roundSegments + 1);
                        int nextA = rootIndexNext + j;
                        int nextB = rootIndexNext + (j + 1) % (roundSegments + 1);
                        
                        triIndices.Add(nextA);
                        triIndices.Add(currentA);
                        triIndices.Add(currentB);
                        
                        triIndices.Add(nextB);
                        triIndices.Add(nextA);
                        triIndices.Add(currentB);
                    }
                }
            }
            
            //Leaf vine placement
            for (int i = 0; i < path.NumSegments - 1; i++)
            {
                for (int j = -1; j < 2; j += 2)
                {
                    int amountVineVertice = verts.Count;

                    Vector3 upLeaf = Vector3.Lerp(points[i * 3].rot * Vector3.up, points[(i + 1) * 3].rot * Vector3.up, 0);
                    Vector3 upLeafNext = Vector3.Lerp(points[i * 3].rot * Vector3.up, points[(i + 1) * 3].rot * Vector3.up, 0.5f);
                    OrientedPoint opLeaf = GetBezierPoint(0, i, upLeaf);
                    OrientedPoint opLeafNext = GetBezierPoint(0.5f, i, upLeafNext);

                    float vineSize = vineSizeCurve.Evaluate((float)i / (path.NumSegments - 1));
                    float vineSizeNext = vineSizeCurve.Evaluate((float)(i + 1) / (path.NumSegments - 1));

                    Vector3 vineOffset = opLeaf.LocalToWorldPosition(Vector3.up * (vineDiameter * vineSize));
                    Vector3 vineOffsetNext = opLeafNext.LocalToWorldPosition(Vector3.up * (vineDiameter * vineSizeNext));

                    float x = Vector3.Distance(vineOffset, vineOffsetNext);
                    
                    Vector3 vineOffsetRight = opLeafNext.LocalToWorldPosition(new Vector3(j * x, 0, 0) + Vector3.up * (vineDiameter * vineSizeNext));
                    Vector3 vineOffsetNextRight = opLeaf.LocalToWorldPosition(new Vector3(j * x, 0, 0) + Vector3.up * (vineDiameter * vineSize));
                    
                    verts.Add(removeTransform(vineOffset));
                    verts.Add(removeTransform(vineOffsetNext));
                    verts.Add(removeTransform(vineOffsetRight));
                    verts.Add(removeTransform(vineOffsetNextRight));
                
                    uvs.Add(new Vector2(1, 0));
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(1, 1));
                
                    for (int k = 0; k < 4; k++)
                    {
                        normals.Add(opLeaf.rot * Vector3.up);
                    }

                    if (j == -1)
                    {
                        leafTriangles.Add(amountVineVertice + 2);
                        leafTriangles.Add(amountVineVertice + 1);
                        leafTriangles.Add(amountVineVertice);
            
                        leafTriangles.Add(amountVineVertice);
                        leafTriangles.Add(amountVineVertice + 3);
                        leafTriangles.Add(amountVineVertice + 2);
                        continue;
                    }
                    leafTriangles.Add(amountVineVertice);
                    leafTriangles.Add(amountVineVertice + 1);
                    leafTriangles.Add(amountVineVertice + 2);
            
                    leafTriangles.Add(amountVineVertice + 2);
                    leafTriangles.Add(amountVineVertice + 3);
                    leafTriangles.Add(amountVineVertice);
                }
            }
            amountVineVertices = verts.Count;
        }
        
        mesh.subMeshCount = 2;
        
        mesh.SetVertices(verts);
        mesh.SetTriangles(triIndices, 0);
        mesh.SetTriangles(leafTriangles, 1);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
    }

    private Vector3 removeTransform(Vector3 pos)
    {
        pos -= transform.position;
        pos = Quaternion.Inverse(transform.rotation) * pos;
        Vector3 scale = transform.localScale;
        pos = new Vector3(pos.x / scale.x, pos.y / scale.y, pos.z / scale.z);

        return pos;
    }
    
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
