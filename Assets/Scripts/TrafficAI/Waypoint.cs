using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("Waypoint Status")]
    public Waypoint previousWaypoint;
    public Waypoint nextWaypoint;

    [Range(0f, 5f)]
    public float waypointWidth = 1f;
    public List<Waypoint> branches=new List<Waypoint>();

    [Range(0f,1f)]
    public float branchRatio = 0.5f;

    public Vector3 GetPosition()
    {
        Vector3 minBound = transform.position + transform.right * waypointWidth / 2f;
        Vector3 maxBound = transform.position - transform.forward * waypointWidth / 2f;

        return Vector3.Lerp(minBound,maxBound, Random.Range(0f, 1f));
    }

}
