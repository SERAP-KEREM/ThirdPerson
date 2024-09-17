using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    [Header("NPC Character")]
    public CharacterNavigatorScript character;
    public Waypoint currentWaypoint;
    int direction;


    private void Awake()
    {
        character = GetComponent<CharacterNavigatorScript>();
       // currentWaypoint = GetComponent<Waypoint>();
    }
    private void Start()
    {
        direction = Mathf.RoundToInt(Random.Range(0f, 1f));
        character.LocalDestination (currentWaypoint.GetPosition());
    }

    private void Update()
    {
        if (character.destinationReached)
        {
            bool shouldBranch = false;

            if (currentWaypoint.branches != null && currentWaypoint.branches.Count > 0)
            {
                shouldBranch = Random.Range(0f, 1f) <= currentWaypoint.branchRatio ? true : false;
            }

            if (shouldBranch)
            {
                currentWaypoint = currentWaypoint.branches[Random.Range(0, currentWaypoint.branches.Count - 1)];
            }
            else
            {
                if (direction == 0)
                {
                    if(currentWaypoint.nextWaypoint !=null)
                    {
                        currentWaypoint = currentWaypoint.nextWaypoint;
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                        direction = 1;
                    }
                }
                else if (direction == 1)
                {
                    if (currentWaypoint.previousWaypoint != null)
                    {
                        currentWaypoint = currentWaypoint.previousWaypoint;
                    }
                    else
                    {
                        currentWaypoint = currentWaypoint.nextWaypoint;
                        direction = 0;
                    }
                }
            }
            character.LocalDestination(currentWaypoint.GetPosition());
        }
    }
}
