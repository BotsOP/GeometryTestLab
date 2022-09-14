using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Vines : MonoBehaviour
{
	[SerializeField] private ComputeShader computeShader;
	[SerializeField] [Range(4, 64)]private int roundSegments = 16;
	[SerializeField] [Range(2, 16)] private int bezierSubSegments = 3;
	[SerializeField] private Transform[] transformArray;
	
	[SerializeField] private bool updateMesh = true;

	private int cachedRoundSegments;
	float localTime;
	private Mesh newMesh;

	private int TotalVerts => 2 * roundSegments + (bezierSubSegments - 2) * roundSegments;
	private int TotalIndice => TotalVerts * 3;

	// Buffers for GPU compute shader path
	GraphicsBuffer gpuVertices;
	GraphicsBuffer gpuIndice;
	ComputeBuffer pathPoints;
	
	void OnEnable()
	{
		CreateMesh();
		pathPoints = new ComputeBuffer(4, sizeof(float) * 16);
	}

	void OnDisable()
	{
		gpuVertices?.Dispose();
		gpuVertices = null;
		gpuIndice?.Dispose();
		gpuIndice = null;
	}

	private void OnDrawGizmos()
	{
		Handles.DrawBezier(transformArray[0].position, transformArray[3].position, 
			transformArray[1].position, transformArray[2].position,
			Color.green, EditorGUIUtility.whiteTexture, 1f
			);
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
			UpdateVineGpu();
		}
	}
	
	void UpdateVineGpu()
	{
		gpuVertices ??= newMesh.GetVertexBuffer(0);
		gpuIndice ??= newMesh.GetIndexBuffer();
		
		pathPoints.SetData(createMatrixArray(transformArray));
		Matrix4x4[] test = new Matrix4x4[4];
		pathPoints.GetData(test);
		
		computeShader.SetFloat("gTime", localTime);
		computeShader.SetInt("roundSegments", roundSegments);
		computeShader.SetInt("bezierSubSegments", bezierSubSegments);
		
		computeShader.SetBuffer(0, "pathPoints", pathPoints);
		computeShader.SetBuffer(0, "bufVertices", gpuVertices);
		computeShader.SetBuffer(1, "bufIndice", gpuIndice);
		computeShader.Dispatch(0, (newMesh.vertexCount + 64 - 1) / 64, 1, 1);
		computeShader.Dispatch(1, (roundSegments + 32 - 1) / 32, 1, 1);

		Vector3[] vertex = new Vector3[TotalVerts];
		gpuVertices.GetData(vertex);
		int[] index = new int[TotalIndice];
		gpuIndice.GetData(index);
	}
	
	private void UpdateMesh()
	{
		List<Vector3> vertPos = new List<Vector3>();
		for (int i = 0; i < TotalVerts; i++)
		{
			vertPos.Add(Vector3.zero);
		}
		
		int[] indice = new int[TotalIndice];
		for (int i = 0; i < TotalIndice; i++)
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

	private void CreateMesh()
	{
		newMesh = new Mesh();
		
		newMesh.indexFormat = IndexFormat.UInt32;
		
		newMesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		newMesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;

		int amountVertices = TotalVerts;
		
		List<Vector3> vertPos = new List<Vector3>();
		for (int i = 0; i < 2 * amountVertices; i++)
		{
			vertPos.Add(Vector3.zero);
		}

		int[] indice = new int[TotalIndice];
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

	private Matrix4x4[] createMatrixArray(Transform[] transforms)
	{
		Matrix4x4[] matrices = new Matrix4x4[transforms.Length];
		for (int i = 0; i < transforms.Length; i++)
		{
			Quaternion rotation = transforms[i].rotation;
			Vector3 pos = transforms[i].position;
			matrices[i] = new Matrix4x4(
				rotation * new Vector4(1, 0, 0, 0),
				rotation * new Vector4(0, 1, 0, 0),
				rotation * new Vector4(0, 0, 1, 0),
				new Vector4(pos.x, pos.y, pos.z , 1)
			);
		}

		return matrices;
	}
}
