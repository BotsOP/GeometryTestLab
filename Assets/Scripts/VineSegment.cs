using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VineSegment : MonoBehaviour
{
    [Range(4, 32)]
    [SerializeField] private int roundSegments = 16;

    [HideInInspector] public Path path;
    [SerializeField] private List<Transform> transformPoints;

    [SerializeField] private GameObject emptyObject;
    private void OnDrawGizmos()
    {
        path = new Path(transform.position);
        
        // Vector3 pos = Vector3.up;
        // for (int i = 0; i < roundSegments; i++)
        // {
        //     float amountDegrees = 360 / roundSegments * i;
        //     Vector3 newPos = Quaternion.Euler(amountDegrees, 0, 0) * pos;
        //     Gizmos.DrawSphere(newPos, 0.05f);
        // }
        
        int transformPointCount = transformPoints.Count;
        
        if (path.NumPoints > transformPointCount)
        {
            Debug.Log("not enough points  " + path.NumPoints + "  " + transformPoints.Count);
            for (int i = 0; i < path.NumPoints - transformPointCount; i++)
            {
                GameObject newObject =
                    Instantiate(emptyObject, path[transformPointCount + i], Quaternion.identity, transform);
                
                var iconContent = EditorGUIUtility.IconContent("Assets/Textures/emptyImage.png");
                EditorGUIUtility.SetIconForObject(newObject, (Texture2D) iconContent.image);
                
                transformPoints.Add(newObject.transform);
            }
        }
        
        else if (path.NumPoints < transformPoints.Count)
        {
            Debug.Log("too many points  " + path.NumPoints + "  " + transformPoints.Count);
            for (int i = 0; i < transformPoints.Count - path.NumPoints; i++)
            {
                DestroyImmediate(transformPoints[transformPoints.Count - path.NumPoints + i]);
                transformPoints.Remove(transformPoints[transformPoints.Count - path.NumPoints + i]);
            }
        }

        for (int i = 0; i < path.NumPoints; i++)
        {
            path.MovePoint(i, transformPoints[i].position);
        }

        for (int i = 0; i < path.NumSegments; i++)
        {
            int startIndex = i * 3;
            Handles.DrawBezier(path[startIndex], 
                path[startIndex + 3], 
                path[startIndex + 1], 
                path[startIndex + 2], Color.white, EditorGUIUtility.whiteTexture, 1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(path[startIndex], path[startIndex + 1]);
            Gizmos.DrawLine(path[startIndex + 2], path[startIndex + 3]);
            Gizmos.color = Color.white;
        }

        for (int i = 0; i < path.NumPoints; i++)
        {
            if (i % 3 != 0)
            {
                Gizmos.color = Color.red;
            }
            Gizmos.DrawSphere(path[i], 0.05f);
            Gizmos.color = Color.white;
        }
        
        Event guiEvent = Event.current;

        if (guiEvent.button == 2 && guiEvent.isMouse)
        {
            while (transform.childCount > 0)
            {
                foreach (Transform child in transform) {
                    DestroyImmediate(child.gameObject);
                }
            }
            transformPoints.Clear();
            path = new Path(transform.position);
        }
        // if (guiEvent.button == 0 && guiEvent.shift)
        // {
        //     Vector3 anchorPos = GetBezierPoint(1, path.NumSegments).rot * Vector3.forward + path[path.NumPoints - 1];
        //     path.AddSegment(anchorPos);
        // }
        
    }
    
    OrientedPoint GetBezierPoint(float t, int segment)
    {
        Vector3[] points = path.GetPointsInSegment(segment);
        Vector3 p0 = points[0];
        Vector3 p1 = points[1];
        Vector3 p2 = points[2];
        Vector3 p3 = points[3];

        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        Vector3 pos = Vector3.Lerp(d, e, t);
        Vector3 tangent = (e - d).normalized;

        return new OrientedPoint(pos, tangent);
    }

    private void Awake()
    {
        
    }
}
