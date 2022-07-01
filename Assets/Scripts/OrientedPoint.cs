using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OrientedPoint
{
    public Vector3 pos;
    public Quaternion rot;

    public OrientedPoint(Vector3 pos, Quaternion rot)
    {
        this.pos = pos;
        this.rot = rot;
    }
        
    public OrientedPoint(Vector3 pos, Vector3 forward, Vector3 up)
    {
        this.pos = pos;
        rot = Quaternion.LookRotation(forward, up);
    }
        
    public OrientedPoint(Vector3 pos, Vector3 forward)
    {
        this.pos = pos;
        rot = Quaternion.LookRotation(forward);
    }

    public Vector3 LocalToWorldPosition(Vector3 localPos)
    {
        return pos + rot * localPos;
    }
        
    public Vector3 LocalToWorldVect(Vector3 localPos)
    {
        return rot * localPos;
    }
}
