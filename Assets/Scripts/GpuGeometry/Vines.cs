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
	[SerializeField] [Range(4, 256)]private int roundSegments = 16;
	[SerializeField] [Range(2, 256)] private int bezierSubSegments = 3;
	[SerializeField] [Range(1, 64)] private int bezierSegments = 3;
	[SerializeField] private Transform[] transformArray;
	
	[SerializeField] private bool updateMesh = true;

	private int cachedRoundSegments;
	private int cachedBezierSubSegments;
	private int cachedBezierSegments;
	float localTime;
	private Mesh newMesh;

	private int TotalVerts => (2 * roundSegments + (bezierSubSegments - 2) * roundSegments) * bezierSegments;
	private int TotalIndice => TotalVerts * 6 - (roundSegments * 6);

	// Buffers for GPU compute shader path
	GraphicsBuffer gpuVertices;
	GraphicsBuffer gpuIndice;
	ComputeBuffer pathPoints;
	
	void OnEnable()
	{
		CreateMesh();
		pathPoints = new ComputeBuffer(transformArray.Length, sizeof(float) * 16);
	}

	void OnDisable()
	{
		gpuVertices?.Dispose();
		gpuVertices = null;
		gpuIndice?.Dispose();
		gpuIndice = null;
		pathPoints?.Dispose();
		pathPoints = null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		for (int i = 0; i < transformArray.Length; i +=3)
		{
			Gizmos.DrawSphere(transformArray[i].position, 0.5f);
		}

		Gizmos.color = Color.red;
		for (int i = 1; i < transformArray.Length; i++)
		{
			if(i % 3 == 0) continue;
			Gizmos.DrawSphere(transformArray[i].position, 0.5f);
		}
		
		Handles.DrawBezier(transformArray[0].position, transformArray[3].position, 
			transformArray[1].position, transformArray[2].position,
			Color.green, EditorGUIUtility.whiteTexture, 1f
			);
		Handles.DrawBezier(transformArray[3].position, transformArray[6].position, 
			transformArray[4].position, transformArray[5].position,
			Color.green, EditorGUIUtility.whiteTexture, 1f
		);
		Handles.DrawBezier(transformArray[6].position, transformArray[9].position, 
			transformArray[7].position, transformArray[8].position,
			Color.green, EditorGUIUtility.whiteTexture, 1f
		);
	}

	public void Update()
	{
		if (roundSegments != cachedRoundSegments || bezierSubSegments != cachedBezierSubSegments || bezierSegments != cachedBezierSegments)
		{
			UpdateMesh();
			cachedRoundSegments = roundSegments;
			cachedBezierSubSegments = bezierSubSegments;
			cachedBezierSegments = bezierSegments;
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
		computeShader.SetInt("bezierSegments", bezierSegments);
		
		computeShader.SetBuffer(0, "pathPoints", pathPoints);
		computeShader.SetBuffer(0, "bufVertices", gpuVertices);
		computeShader.Dispatch(0, (TotalVerts + 64 - 1) / 64, 1, 1);
		
		computeShader.SetBuffer(1, "bufIndice", gpuIndice);
		computeShader.Dispatch(1, (TotalVerts + 32 - 1) / 32, 1, 1);
		
		Vector3[] vertex = new Vector3[TotalVerts];
		gpuVertices.GetData(vertex);
		int[] index = new int[TotalIndice];
		gpuIndice.GetData(index);
		float t = 1.0f % 1.01f;
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
		
		newMesh.bounds = new Bounds(transform.position, new Vector3(500, 500, 500));
		
		gpuVertices?.Dispose();
		gpuVertices = null;
		gpuIndice?.Dispose();
		gpuIndice = null;
		
		gpuVertices = newMesh.GetVertexBuffer(0);
		gpuIndice = newMesh.GetIndexBuffer();
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
		
		newMesh.SetVertexBufferParams(newMesh.vertexCount, 
			new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0), 
						new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 1)
			);
		
		newMesh.SetVertices(vertPos);
		newMesh.triangles = indice;
		newMesh.name = "vines";
		newMesh.bounds = new Bounds(transform.position, new Vector3(500, 500, 500));

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
