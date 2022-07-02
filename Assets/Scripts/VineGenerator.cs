using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [SerializeField] private VineSegment vines;
    
    [HideInInspector] public List<OrientedPoint> points = new();
    
    private readonly Vector3[] dirs = 
    {
        Vector3.forward,
        Vector3.down,
        Vector3.back,
        Vector3.up
    };
    private void Update()
    {
        points.Clear();

        Random.seed = randomSeed;
        GetVineGrowth();

        vines.points = points;
    }

    private void OnDrawGizmos()
    {
        points.Clear();

        Random.seed = randomSeed;
        GetVineGrowth();

        vines.points = points;
    }

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
            
            //Debug.DrawRay(currentOP.pos, currentOP.rot * Vector3.up * 0.1f);
            //Gizmos.DrawSphere(currentOP.pos, 0.05f);
        }

        Vector3 lastDir = Vector3.zero;
        Vector3 lastPos = Vector3.zero;
        Quaternion lastRot = points[0].rot;
        
        Gizmos.color = Color.blue;
        int totalPoints = points.Count - 1;
        
        points.Insert(1, points[0]);
        
        for (int i = 0; i < totalPoints - 1; i++)
        {
            Vector3 posA = points[i * 3].pos;
            Vector3 posB = points[i * 3 + 2].pos;
            Vector3 posC = points[i * 3 + 3].pos;

            Vector3 dirAB = (posB - posA).normalized;
            Vector3 dirCB = posC - posB;
            dirAB *= dirCB.magnitude;

            Vector3 posD = posB + dirAB;
            Vector3 dirDC = posC - posD;
            Vector3 posE = posD + dirDC * 0.5f;
            Vector3 dirEB = posB - posE;

            Vector3 handleA = posB + dirEB * (dirAB.magnitude * 0.9f);
            Vector3 handleB = posB - dirEB * (dirCB.magnitude * 0.9f);
            
            //Gizmos.DrawSphere(handleA, 0.04f);
            //Gizmos.DrawSphere(handleB, 0.04f);
            
            // Debug.DrawRay(posA, dirAB);
            // Debug.DrawRay(posB, dirCB);
            // Debug.DrawRay(posB, dirEB * (dirAB.magnitude * 0.5f));
            // Debug.DrawRay(posB, dirEB * (dirCB.magnitude * 0.5f * -1));

            points.Insert(i * 3 + 2,new OrientedPoint(handleA, lastRot));
            points.Insert(i * 3 + 4,new OrientedPoint(handleB, lastRot));
            
            // Gizmos.DrawSphere(handleA, 0.04f);
            // Gizmos.DrawSphere(lastPos, 0.04f);
        }
        
        points.Add(points[^1]);
        
        // Handles.DrawBezier(points[0].pos, points[3].pos, points[1].pos, points[2].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[3].pos, points[6].pos, points[4].pos, points[5].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[6].pos, points[9].pos, points[7].pos, points[8].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[9].pos, points[12].pos, points[10].pos, points[11].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        // Handles.DrawBezier(points[12].pos, points[15].pos, points[13].pos, points[14].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        
        //points.Clear();
        
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
        Vector3 originalPos = origin.pos;
        //update this to use hit.normal
        //origin.pos = origin.LocalToWorldPosition(Vector3.up * 0.1f);

        float randomAngle = Random.Range(-1 * (randomMaxAngle - randomMinAngle), randomMaxAngle - randomMinAngle) + randomMinAngle;
        origin.rot *= Quaternion.Euler(0, randomAngle, 0);

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = (origin.rot * dirs[i] * stepSize);
            //Debug.DrawRay(origin.pos, dir, Color.blue);
            RaycastHit hit;
            if (Physics.Raycast(origin.pos + origin.rot * Vector3.up * 0.1f, dir, out hit, stepSize))
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(hit.point, 0.02f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(origin.pos + origin.rot * Vector3.up * 0.1f, 0.01f);
                Debug.DrawRay(origin.pos + origin.rot * Vector3.up * 0.1f, dir, Color.red);
                
                //Debug.DrawRay(origin.pos, origin.rot * Vector3.up * 0.1f, Color.green);
                origin.pos = hit.point - dir * 0.01f;
                
                //origin.rot = Quaternion.LookRotation(Quaternion.AngleAxis(-90, transform.rotation * Vector3.left) * hit.normal, hit.normal);
                origin.rot = Quaternion.LookRotation(hit.point - originalPos, hit.normal);
                //Debug.DrawRay(origin.pos, origin.rot * Vector3.up * 0.15f, Color.cyan);
                //Debug.DrawRay(origin.pos, origin.rot * Vector3.forward * 0.15f, Color.red);
                break;
            }
            //Debug.DrawRay(origin.pos, dir, Color.white);
            origin.pos += dir;
        }
        return origin;
    }
}
