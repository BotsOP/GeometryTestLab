using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tube : MonoBehaviour
{
	[SerializeField] private ComputeShader computeShader;
	[SerializeField] private int roundSegments = 16;
	private int cachedRoundSegments;
	
	[SerializeField] private bool updateMesh = true;

	float localTime;
	private Mesh newMesh;
	
	// Buffers for GPU compute shader path
	GraphicsBuffer gpuVertices;
	GraphicsBuffer gpuIndice;
	GraphicsBuffer gpuOldVertices;
	
	void OnEnable()
	{
		CreateMesh();
	}

	void OnDisable()
	{
		gpuVertices?.Dispose();
		gpuVertices = null;
		gpuIndice?.Dispose();
		gpuIndice = null;
	}

	private void UpdateMesh()
	{
		List<Vector3> vertPos = new List<Vector3>();
		for (int i = 0; i < 2 * roundSegments; i++)
		{
			vertPos.Add(Vector3.zero);
		}
		
		int[] indice = new int[roundSegments * 2 * 3];
		for (int i = 0; i < indice.Length; i++)
		{
			indice[i] = 0;
		}
		
		newMesh.SetVertices(vertPos);
		newMesh.triangles = indice;
		
		gpuVertices?.Dispose();
		gpuVertices = null;
		gpuIndice?.Dispose();
		gpuIndice = null;
		
		gpuVertices = newMesh.GetVertexBuffer(0);
		gpuIndice = newMesh.GetIndexBuffer();
		//GetComponent<MeshFilter>().mesh = newMesh;
	}

	public void Update()
	{
		if (roundSegments != cachedRoundSegments)
		{
			UpdateMesh();
			cachedRoundSegments = roundSegments;
		}
		
		if (updateMesh)
		{
			localTime += Time.deltaTime * 2.0f;
			UpdateWaveGpu();
		}
	}
	
	void UpdateWaveGpu()
	{
		gpuVertices ??= newMesh.GetVertexBuffer(0);
		gpuIndice ??= newMesh.GetIndexBuffer();
		//Debug.Log($"{gpuVertices.count}  {gpuIndice.count}  {RoundSegments}  {newMesh.vertexCount}");
		
		computeShader.SetFloat("gTime", localTime);
		computeShader.SetInt("roundSegments", roundSegments);
		
		computeShader.SetBuffer(0, "bufVertices", gpuVertices);
		computeShader.SetBuffer(1, "bufIndice", gpuIndice);
		computeShader.Dispatch(0, newMesh.vertexCount, 10, 1);
		computeShader.Dispatch(1, roundSegments, 1, 1);

		Vector3[] vertex = new Vector3[2 * roundSegments];
		gpuVertices.GetData(vertex);
		int[] index = new int[roundSegments * 2 * 3];
		gpuIndice.GetData(index);
		
	}

	private void CreateMesh()
	{
		newMesh = new Mesh();
		
		newMesh.indexFormat = IndexFormat.UInt32;
		
		newMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		newMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
		
		List<Vector3> vertPos = new List<Vector3>();
		for (int i = 0; i < 2 * roundSegments; i++)
		{
			vertPos.Add(Vector3.zero);
		}

		int[] indice = new int[roundSegments * 2 * 3];
		for (int i = 0; i < indice.Length; i++)
		{
			indice[i] = 0;
		}
		
		//newMesh.SetVertexBufferParams(newMesh.vertexCount, new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0));
		
		newMesh.SetVertices(vertPos);
		newMesh.triangles = indice;
		newMesh.name = "vines";
		GetComponent<MeshFilter>().mesh = newMesh;
	}
}
