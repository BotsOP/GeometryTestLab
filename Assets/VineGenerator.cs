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
        Vector3.forward,
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
        List<OrientedPoint> points = new List<OrientedPoint>();
        OrientedPoint currentOP = new OrientedPoint(transform.position, transform.rotation);
        points.Add(currentOP);
        Gizmos.DrawSphere(currentOP.pos, 0.05f);
        for (int i = 0; i < vinePoints; i++)
        {
            currentOP = CalculateNextPoint(currentOP);
            Gizmos.DrawSphere(currentOP.pos, 0.05f);
        }
    }

    private OrientedPoint CalculateNextPoint(OrientedPoint origin)
    {
        origin.pos = origin.LocalToWorldPosition(Vector3.up * 0.02f);

        for (int i = 0; i < dirs.Length; i++)
        {
            // Vector3 dir = direction * dirs[i] * stepSize;
            // dir = origin.rot * dir;

            Vector3 dir = (origin.rot * dirs[i] * stepSize);
            
            RaycastHit hit;
            if (Physics.Raycast(origin.pos, dir, out hit, stepSize))
            {
                //Debug.DrawRay(origin.pos, dir * 0.1f, Color.red);
                //Debug.Log("hit something");
                //Debug.DrawRay(origin.pos, origin.rot * Vector3.up * 0.1f, Color.green);
                origin.pos = hit.point - dir * 0.01f;
                
                origin.rot = Quaternion.LookRotation(Quaternion.AngleAxis(-90, transform.rotation * Vector3.left) * hit.normal, hit.normal);
                //Debug.DrawRay(origin.pos, Quaternion.AngleAxis(-90, transform.rotation * Vector3.left) * hit.normal * 0.1f, Color.cyan);
                break;
            }
            //Debug.DrawRay(origin.pos, dir, Color.white);
            origin.pos += dir;
        }
        return origin;
    }
}
