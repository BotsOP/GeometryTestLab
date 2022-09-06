using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class Tube : MonoBehaviour
{
    [SerializeField] private ComputeShader vineCalculator;

    [SerializeField] private int roundSegments = 4;
    [SerializeField] private int bezierSegments = 3;
    [SerializeField] private int bezierSubSegments = 5;

    private Mesh newMesh;
    private int kernelIndex;

    private ComputeBuffer pathPoints;
    private GraphicsBuffer gpuVertices;
    private GraphicsBuffer gpuIndice;
    private void OnDrawGizmos()
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < bezierSegments * 3 + 1; i++)
        {
            Vector3 pos = transform.GetChild(i).position;
            points.Add(pos);
            Gizmos.DrawSphere(pos, 0.5f);
        }

        for (int i = 0; i < bezierSegments; i++)
        {
            int index = 3 * i;
            Handles.DrawBezier(points[index], points[index + 3], points[index + 1], 
                points[index + 2], Color.green, EditorGUIUtility.whiteTexture, 1f);
        }
        
        points.Clear();
    }

    private void OnEnable()
    {
        pathPoints = new ComputeBuffer(bezierSegments * 3 + 1, sizeof(float) * 3);
        kernelIndex = vineCalculator.FindKernel("CSMain");
    }

    private void Start()
    {
        newMesh = new Mesh();
        newMesh.indexFormat = IndexFormat.UInt32;

        newMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
        newMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
        

        Vector3[] vertices = new Vector3[(bezierSegments + 1) * roundSegments * bezierSubSegments];
        
        // newMesh.SetVertexBufferParams(vertices.Length, new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0));

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.zero;
        }

        int[] indices = new int[(bezierSegments + 1) * roundSegments * bezierSubSegments * 3 - 3];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = 0;
        }
        newMesh.SetVertices(vertices);
        newMesh.SetTriangles(indices, 0);
        newMesh.RecalculateNormals();
        newMesh.name = "test";

        GetComponent<MeshFilter>().sharedMesh = newMesh;
        
        
    }

    private void Update()
    {
        Vector3[] pointArray = new Vector3[bezierSegments * 3 + 1];
        for (int i = 0; i < bezierSegments * 3 + 1; i++)
        {
            Vector3 pos = transform.GetChild(i).position;
            pointArray[i] = pos;
        }
        pathPoints.SetData(pointArray);
        
        gpuVertices ??= newMesh.GetVertexBuffer(0);
        gpuIndice ??= newMesh.GetIndexBuffer();
        
        vineCalculator.SetInt("roundSegments", roundSegments);
        vineCalculator.SetBuffer(kernelIndex, "bufVertices", gpuVertices);
        vineCalculator.SetBuffer(kernelIndex, "bufIndices", gpuIndice);
        vineCalculator.SetBuffer(kernelIndex, "pathPoints", pathPoints);
        
        vineCalculator.Dispatch(kernelIndex, (bezierSegments + 1), 1, 1);

        List<Vector3> vertice = new List<Vector3>();
        newMesh.GetVertices(vertice);
        // gpuVertices.GetData();
        Debug.Log(vertice[0]);
        Debug.Log(vertice[1]);
        Debug.Log(vertice[2]);
        Debug.Log(vertice[3]);
    }

    private void OnDestroy()
    {
        gpuVertices?.Dispose();
        gpuVertices = null;
        gpuIndice?.Dispose();
        gpuIndice = null;
    }
    
    private struct orPoint
    {
        private Vector3 position;

        orPoint(Vector3 _position)
        {
            position = _position;
        }
    }
}
