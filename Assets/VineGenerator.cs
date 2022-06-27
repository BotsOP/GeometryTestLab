using System.Collections.Generic;
using UnityEngine;

public class VineGenerator : MonoBehaviour
{
    [Range(1, 64)]
    [SerializeField] private int vinePoints;
    [Range(0.1f, 2)]
    [SerializeField] private float stepSize;
    private readonly Vector3[] dirs = 
    {
        Vector3.up,
        Vector3.forward,
        Vector3.down,
        Vector3.down,
        Vector3.back,
        Vector3.up
    };
    private void Update()
    {
        //Debug.DrawRay(new Vector3(10, 0 , 0), transform.TransformDirection(Vector3.forward));
        //CalculateNextPoint(transform.position, transform.rotation);
    }

    private void OnDrawGizmos()
    {
        GetVineGrowth();
    }

    private void GetVineGrowth()
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 currentPos = transform.position;
        points.Add(currentPos);
        Gizmos.DrawSphere(currentPos, 0.05f);
        for (int i = 0; i < vinePoints; i++)
        {
            currentPos = CalculateNextPoint(currentPos, transform.rotation);
            Gizmos.DrawSphere(currentPos, 0.05f);
        }
    }

    private Vector3 CalculateNextPoint(Vector3 origin, Quaternion direction)
    {
        Vector3 newPos = Vector3.zero;
        
        for (int i = 0; i < dirs.Length; i++)
        {
            RaycastHit hit;
            Vector3 dir = direction * dirs[i] * stepSize;
            if (Physics.Raycast(origin, dir, out hit, stepSize))
            {
                Debug.DrawRay(origin, dir, Color.red);
                //Debug.Log("hit something");
                newPos = hit.point - dir * 0.01f;
                break;
            }
            Debug.DrawRay(origin, dir, Color.white);
            origin += dir;
        }
        return newPos;
    }
}
