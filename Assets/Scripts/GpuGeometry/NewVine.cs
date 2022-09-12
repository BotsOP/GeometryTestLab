using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

// Simple water wave procedural mesh based on https://www.konsfik.com/procedural-water-surface-made-in-unity3d/ - written by Kostas Sfikas, March 2017.
// Note that this sample shows both CPU and GPU mesh modification approaches, and some of the code complexity is
// because of that; comments point out these places.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class NewVine : MonoBehaviour
{
	[SerializeField] private ComputeShader computeShader;
	[SerializeField] private Mesh newMesh;
	[SerializeField] private bool updateMesh;

	Mesh mesh;
	float localTime;
	
	// Buffers for GPU compute shader path
	GraphicsBuffer gpuVertices;
	GraphicsBuffer gpuIndice;
	GraphicsBuffer gpuOldVertices;
	
	void OnEnable()
	{
		mesh = CreateMesh();
	}

	void OnDisable()
	{
		gpuVertices?.Dispose();
		gpuVertices = null;
	}

	public void Update()
	{
		if (updateMesh)
		{
			localTime += Time.deltaTime * 2.0f;
			UpdateWaveGpu();
		}
	}
	
	void UpdateWaveGpu()
	{
		// Create GPU buffer for wave source positions, if needed.
		gpuVertices ??= mesh.GetVertexBuffer(0);
		gpuIndice ??= mesh.GetIndexBuffer();
		
		computeShader.SetFloat("gTime", localTime);
		// update vertex positions
		computeShader.SetBuffer(0, "bufVertices", gpuVertices);
		computeShader.SetBuffer(0, "bufIndice", gpuIndice);
		computeShader.SetBuffer(0, "oldVertice", gpuOldVertices);
		computeShader.Dispatch(0, 240, 1, 1);
	}

	Mesh CreateMesh()
	{
		newMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		newMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
		
		GetComponent<MeshFilter>().mesh = newMesh;
		
		List<Vector3> vertPos = new List<Vector3>();
		newMesh.GetVertices(vertPos);
		
		gpuOldVertices = new GraphicsBuffer(GraphicsBuffer.Target.Structured, newMesh.vertexCount, 12);
		gpuOldVertices.SetData(vertPos);
		computeShader.SetBuffer(0, "oldVertice", gpuOldVertices);
		newMesh.SetVertexBufferParams(newMesh.vertexCount, new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0));
		
		return newMesh;
	}
}
