using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoTest : MonoBehaviour
{
    public Vector2 vec;
    public Transform toTransform;
    [Range(0, 360)] public float angleX;
    private void OnDrawGizmos()
    {
        vec = vec.normalized;

        float x = (float)(vec.magnitude * Math.Cos((angleX * Mathf.PI)/180));
        float z = (float)(vec.magnitude * Math.Sin((angleX * Mathf.PI)/180));
        Vector3 newPos = new Vector3(x, z, 0);
        newPos = CreateMatrix(toTransform) * new Vector4(newPos.x, newPos.y, newPos.z, 1);
        
        Gizmos.DrawLine(toTransform.position, newPos);
        Gizmos.DrawSphere(newPos, 0.1f);
    }

    private Matrix4x4 CreateMatrix(Transform transMatrix)
    {
        Matrix4x4 matrix = new Matrix4x4(
            transMatrix.rotation * new Vector4(1, 0, 0, 0),
            transMatrix.rotation * new Vector4(0, 1, 0, 0),
            transMatrix.rotation * new Vector4(0, 0, 1, 0),
            new Vector4(transMatrix.position.x, transMatrix.position.y, transMatrix.position.z, 1)
        );
        return matrix;
    }
}
