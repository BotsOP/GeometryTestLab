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
    [Range(1, 32)]
    [SerializeField] private int amountVines;
    [Range(0.1f, 2)]
    [SerializeField] private float stepSize;
    [Range(0, 90)]
    [SerializeField] private float randomMaxAngle;
    [Range(0, 45)]
    [SerializeField] private float randomMinAngle;
    [Range(0, 1000)]
    [SerializeField] private int randomSeed;

    [SerializeField] private VineSegment vines;

    public List<OrientedPoint>[] pointsArray;
    
    private readonly Vector3[] dirs = 
    {
        Vector3.forward,
        Vector3.down,
        Vector3.back,
        Vector3.up
    };

    private void Start()
    {
        pointsArray = new List<OrientedPoint>[amountVines];
        for (int i = 0; i < pointsArray.Length; i++)
        {
            pointsArray[i] = new List<OrientedPoint>();
        }
        
        for(int i = 0; i < pointsArray.Length; i++)
        {
            Random.seed = randomSeed + i;
            float t = i / (float)pointsArray.Length;
            float angRad = t * MathLibrary.TAU;
            Vector3 dir =  new Vector3(MathLibrary.GetVectorByAngle(angRad).x, 0, MathLibrary.GetVectorByAngle(angRad).y);
            GetVineGrowth(pointsArray[i], Quaternion.LookRotation(dir));
        }
        vines.GenerateMesh(pointsArray);
    }

    // private void Update()
    // {
    //     ClearListArray(pointsArray);
    //
    //     for(int i = 0; i < pointsArray.Length; i++)
    //     {
    //         Random.seed = randomSeed;
    //         float t = i / (float)pointsArray.Length;
    //         float angRad = t * MathLibrary.TAU;
    //         Vector3 dir =  new Vector3(MathLibrary.GetVectorByAngle(angRad).x, 0, MathLibrary.GetVectorByAngle(angRad).y);
    //         GetVineGrowth(pointsArray[i], transform.rotation);
    //     
    //         vines.GenerateMesh(pointsArray[0]);
    //     }
    // }

    private void OnDrawGizmos()
    {
        List<OrientedPoint>[] pointsArrayGizmo = new List<OrientedPoint>[amountVines];
        for (int i = 0; i < pointsArrayGizmo.Length; i++)
        {
            pointsArrayGizmo[i] = new List<OrientedPoint>();
        }
    
        for(int i = 0; i < pointsArrayGizmo.Length; i++)
        {
            Random.seed = randomSeed + i;
            float t = i / (float)pointsArrayGizmo.Length;
            float angRad = t * MathLibrary.TAU;
            Vector3 dir =  new Vector3(MathLibrary.GetVectorByAngle(angRad).x, 0, MathLibrary.GetVectorByAngle(angRad).y);
            GetVineGrowthGizmo(pointsArrayGizmo[i], Quaternion.LookRotation(dir));
        }
    }

    private void GetVineGrowth(List<OrientedPoint> points, Quaternion offsetRotation)
    {
        Gizmos.color = Color.white;
        OrientedPoint currentOP = new OrientedPoint(transform.position, offsetRotation);
        points.Add(currentOP);
        
        for (int i = 0; i < vinePoints; i++)
        {
            currentOP = CalculateNextPoint(currentOP);
            points.Add(currentOP);
        }

        Quaternion lastRot = points[0].rot;
        
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

            points.Insert(i * 3 + 2,new OrientedPoint(handleA, lastRot));
            points.Insert(i * 3 + 4,new OrientedPoint(handleB, lastRot));
        }
        
        points.Add(points[^1]);
    }
    
    private void GetVineGrowthGizmo(List<OrientedPoint> points, Quaternion offsetRotation)
    {
        Gizmos.color = Color.white;
        OrientedPoint currentOP = new OrientedPoint(transform.position, offsetRotation);
        points.Add(currentOP);
        
        for (int i = 0; i < vinePoints; i++)
        {
            currentOP = CalculateNextPoint(currentOP);
            points.Add(currentOP);
            Gizmos.DrawSphere(currentOP.pos, 0.02f);
        }

        Quaternion lastRot = points[0].rot;
        
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

            points.Insert(i * 3 + 2,new OrientedPoint(handleA, lastRot));
            points.Insert(i * 3 + 4,new OrientedPoint(handleB, lastRot));
        }
        
        points.Add(points[^1]);
        for (int curveSegments = 0; curveSegments < (points.Count - 1); curveSegments += 3)
        {
            int rootIndex = curveSegments;
            Handles.DrawBezier(points[rootIndex].pos, points[rootIndex + 3].pos, 
                points[rootIndex + 1].pos, points[rootIndex + 2].pos, Color.white, EditorGUIUtility.whiteTexture, 1f);
        }
    }

    private OrientedPoint CalculateNextPoint(OrientedPoint origin)
    {
        Vector3 originalPos = origin.pos;

        float randomAngle = Random.Range(-1 * (randomMaxAngle - randomMinAngle), randomMaxAngle - randomMinAngle) + randomMinAngle;
        origin.rot *= Quaternion.Euler(0, randomAngle, 0);

        for (int i = 0; i < dirs.Length; i++)
        {
            Vector3 dir = (origin.rot * dirs[i] * stepSize);
            RaycastHit hit;
            if (Physics.Raycast(origin.pos + origin.rot * Vector3.up * 0.1f, dir, out hit, stepSize))
            {
                origin.pos = hit.point - dir * 0.01f;
                
                origin.rot = Quaternion.LookRotation(hit.point - originalPos, hit.normal);
                break;
            }
            //Debug.DrawRay(origin.pos, dir, Color.white);
            origin.pos += dir;
        }
        return origin;
    }
    
    private void ClearListArray(List<OrientedPoint>[] listArray)
    {
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i].Clear();
        }
    }
}
