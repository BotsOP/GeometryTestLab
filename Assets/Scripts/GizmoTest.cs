using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoTest : MonoBehaviour
{
    public Vector2 vec;
    public float angleX;
    public float angleY;
    private void OnDrawGizmos()
    {
        vec = vec.normalized;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(vec.x, vec.y, 0));
        Gizmos.DrawSphere(transform.position + new Vector3(vec.x, vec.y, 0), 0.1f);
        float x = (float)(vec.magnitude * Math.Cos((angleX * Mathf.PI)/180));
        float z = (float)(vec.magnitude * Math.Sin((angleX * Mathf.PI)/180));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(x, z, 0));
        Gizmos.DrawSphere(transform.position + new Vector3(x, z, 0), 0.1f);


        
    }
}
