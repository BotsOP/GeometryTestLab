using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class VineGenerator : MonoBehaviour
{
    [Range(1, 64)]
    [SerializeField] private int vinePoints;
    [Range(0.1f, 2)]
    [SerializeField] private float stepSize;
    [Range(0, 90)]
    [SerializeField] private float randomMaxAngle;
    [Range(0, 45)]
    [SerializeField] private float randomMinAngle;
    [Range(0, 1000)]
    [SerializeField] private int randomSeed;
    public List<OrientedPoint> points = new List<OrientedPoint>();

    
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
        Random.seed = randomSeed;
        GetVineGrowth();
    }

    // private void OnDrawGizmos()
    // {
    //     Random.seed = randomSeed;
    //     GetVineGrowth();
    // }

    private void GetVineGrowth()
    {
        Gizmos.color = Color.white;
        OrientedPoint currentOP = new OrientedPoint(transform.position, transform.rotation);
        points.Add(currentOP);
        //Gizmos.DrawSphere(currentOP.pos, 0.05f);
        for (int i = 0; i < vinePoints; i++)
        {
            currentOP = CalculateNextPoint(currentOP);
            points.Add(currentOP);
            //Gizmos.DrawSphere(currentOP.pos, 0.05f);
        }

        Vector3 lastDir = Vector3.zero;
        Vector3 lastPos = Vector3.zero;
        Quaternion lastRot = points[0].rot;
        
        Gizmos.color = Color.blue;
        int totalPoints = points.Count - 1;
        
        for (int i = 0; i < totalPoints; i++)
        {
            Vector3 posA = points[i * 3].pos;
            Vector3 posB = points[i * 3 + 1].pos;

            Vector3 dirToB = posB - posA;
            
            Vector3 handleA = posA + (lastDir);
            lastPos = posA + dirToB * 0.9f;
            lastDir = dirToB * 0.2f;
            
            points.Insert(i * 3 + 1,   new OrientedPoint(handleA, lastRot));
            points.Insert(i * 3 + 2, new OrientedPoint(lastPos, points[i].rot));
            
            // Gizmos.DrawSphere(handleA, 0.04f);
            // Gizmos.DrawSphere(lastPos, 0.04f);
        }
        
        
        // Handles.DrawBezier(points[0].pos, points[3].pos, points[1].pos, points[2].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[3].pos, points[6].pos, points[4].pos, points[5].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[6].pos, points[9].pos, points[7].pos, points[8].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[9].pos, points[12].pos, points[10].pos, points[11].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        
        
        // Gizmos.color = Color.green;
        // Gizmos.DrawSphere(points[4].pos, 0.05f);
        // Gizmos.DrawSphere(points[7].pos, 0.05f);
        //
        // Gizmos.color = Color.red;
        // Gizmos.DrawSphere(points[5].pos, 0.04f);
        // Gizmos.DrawSphere(points[8].pos, 0.04f);
        // Gizmos.color = Color.white;
    }

    private OrientedPoint CalculateNextPoint(OrientedPoint origin)
    {
        origin.pos = origin.LocalToWorldPosition(Vector3.up * 0.02f);
        float randomAngle = Random.Range(-1 * (randomMaxAngle - randomMinAngle), randomMaxAngle - randomMinAngle) + randomMinAngle;
        origin.rot *= Quaternion.Euler(0, randomAngle, 0);

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = (origin.rot * dirs[i] * stepSize);
            
            RaycastHit hit;
            if (Physics.Raycast(origin.pos, dir, out hit, stepSize))
            {
                //Debug.DrawRay(origin.pos, dir * 0.1f, Color.red);
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
