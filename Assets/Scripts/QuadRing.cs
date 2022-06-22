using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class QuadRing : MonoBehaviour
{
    [Range(0.01f,1)]
    [SerializeField] private float radiusInner;
    [Range(0.01f,1)]
    [SerializeField] private float thickness;
    [Range(3,128)]
    [SerializeField] private int angularSegmentCount = 3;

    [SerializeField] private int waveFrequency;
    [SerializeField] private float waveOffset;
    [SerializeField] private float waveIntensity;

    private float RadiusOuter => radiusInner + thickness;
    private int VertexCount => angularSegmentCount * 2;

    private Mesh mesh;

    private void OnDrawGizmosSelected()
    {
        GizmoLibrary.DrawWireCircle(transform.position, transform.rotation, radiusInner, angularSegmentCount);
        GizmoLibrary.DrawWireCircle(transform.position, transform.rotation, RadiusOuter, angularSegmentCount);
    }

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void Update()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        mesh.Clear();

        int vCount = VertexCount;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        
        for (int i = 0; i < angularSegmentCount + 1; i++)
        {
            float t = i / (float)angularSegmentCount;
            float angRad = t * MathLibrary.TAU;
            Vector2 dir = MathLibrary.GetVectorByAngle(angRad);

            Vector3 zOffset = (Vector3.forward * Mathf.Cos(angRad * waveFrequency + waveOffset)) / waveIntensity;
            
            vertices.Add((Vector3)(dir * RadiusOuter) + zOffset);
            vertices.Add((Vector3)(dir * radiusInner) + zOffset);
            
            // uvs.Add(new Vector2(t, 0));
            // uvs.Add(new Vector2(t, 1));
            
            uvs.Add(dir * 0.5f + Vector2.one * 0.5f);
            uvs.Add(dir * ((radiusInner / RadiusOuter) * 0.5f) + Vector2.one * 0.5f);
        }

        List<int> triangleIndices = new List<int>();
        for (int i = 0; i < angularSegmentCount; i++)
        {
            int rootIndex = i * 2;
            int indexInnerRoot = rootIndex + 1;
            int indexOuterNext = rootIndex + 2;
            int indexInnerNext = rootIndex + 3;
            
            triangleIndices.Add(rootIndex);
            triangleIndices.Add(indexOuterNext);
            triangleIndices.Add(indexInnerNext);
            
            triangleIndices.Add(rootIndex);
            triangleIndices.Add(indexInnerNext);
            triangleIndices.Add(indexInnerRoot);
        }
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangleIndices, 0);
        mesh.RecalculateNormals();
        mesh.SetUVs(0, uvs);
    }
}
