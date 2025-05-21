using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint
{
    private Vector3 position;
    GameObject point;

    public Vector3 Position
    {
        get
        {
            return position;
        }
        set
        {
            position = value;
        }
    }

    public PathPoint nextPoint;

    public PathPoint(Vector3 position)
    {
        this.position = position;
        this.point = new GameObject("Point");
        point.transform.position = position;
        DrawSphere sphere = point.AddComponent<DrawSphere>();
        sphere.Radius = 2;
    }

    public float GetDistance(Vector3 distanceTo) =>
         Vector3.Distance(position, distanceTo);  

    public void DeleteIndicator() => GameObject.Destroy(point);

}
