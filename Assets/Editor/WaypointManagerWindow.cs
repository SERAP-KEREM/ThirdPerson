using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class WaypointManagerWindow : EditorWindow
{
    [MenuItem("Waypoint/Waypoints Editor Tools")]
    public static void ShowWindow()
    {
        GetWindow<WaypointManagerWindow>("Waypoints Editor Tools");
    }

    public Transform waypointOrigin;

    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty("waypointOrigin"));

        if (waypointOrigin == null)
        {
            EditorGUILayout.HelpBox("Please assign a waypoint origin transform. ", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginVertical("box");
            CreateButtons();
            EditorGUILayout.EndVertical();
        }
        obj.ApplyModifiedProperties();
    }
    void CreateButtons()
    {
        if (GUILayout.Button("Create Waypoint"))
        {
            CreateWaypoint();
        }
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<Waypoint>())
        {
            if (GUILayout.Button("Create Waypoint Before"))
            {
                CreateWaypointBefore();
            }
            if (GUILayout.Button("Create Waypoint After"))
            {
                CreateWaypointAfter();
            }
            if(GUILayout.Button("Add Branch Waypoint"))
            {
                CreateBranch();
            }
            if (GUILayout.Button("Remove Waypoint"))
            {
                RemoveWaypoint();
            }
        }
    }

    void CreateWaypoint()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointOrigin.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointOrigin, false);

        Waypoint waypoint = waypointObject.GetComponent<Waypoint>();
        if (waypointOrigin.childCount > 1)
        {
            waypoint.previousWaypoint = waypointOrigin.GetChild(waypointOrigin.childCount - 2).GetComponent<Waypoint>();
            waypoint.previousWaypoint.nextWaypoint = waypoint;

            waypoint.transform.position = waypoint.previousWaypoint.transform.position;
            waypoint.transform.forward = waypoint.previousWaypoint.transform.forward;
        }

        Selection.activeGameObject = waypoint.gameObject;
    }

    void CreateWaypointBefore()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointOrigin.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointOrigin, false);

        Waypoint newWaypoint =waypointObject.GetComponent<Waypoint>();

        Waypoint selectedWaypoint =Selection.activeGameObject.GetComponent<Waypoint>();

        waypointObject.transform.position = selectedWaypoint.transform.position;
        waypointObject.transform.position = selectedWaypoint.transform.forward;

        if(selectedWaypoint.previousWaypoint)
        {
            newWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
            selectedWaypoint.previousWaypoint.nextWaypoint = newWaypoint;

        }

        newWaypoint.nextWaypoint = selectedWaypoint;
        selectedWaypoint.previousWaypoint = newWaypoint;

        newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
        Selection.activeGameObject = newWaypoint.gameObject;
    }

    void CreateWaypointAfter()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointOrigin.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointOrigin, false);

        Waypoint newWaypoint = waypointObject.GetComponent<Waypoint>();

        Waypoint selectedWaypoint=Selection.activeGameObject.GetComponent<Waypoint>();

        waypointObject.transform.position = selectedWaypoint.transform.position;
        waypointObject.transform.position=selectedWaypoint.transform.forward;

        if(selectedWaypoint.nextWaypoint !=null)
        {
            selectedWaypoint.nextWaypoint.previousWaypoint=newWaypoint;
            newWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint.nextWaypoint;

        }

        selectedWaypoint.nextWaypoint=newWaypoint;

        newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
        Selection.activeGameObject=newWaypoint.gameObject;
    }

    void CreateBranch()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointOrigin.childCount, typeof(Waypoint));
        waypointObject.transform.SetParent(waypointOrigin, false);

        Waypoint Waypoint = waypointObject.GetComponent<Waypoint>();

        Waypoint branchedFrom = Selection.activeGameObject.GetComponent<Waypoint>();
        branchedFrom.branches.Add(Waypoint);

        Waypoint.transform.position = branchedFrom.transform.position;
        Waypoint.transform.forward = branchedFrom.transform.forward;

        Selection.activeGameObject = Waypoint.gameObject;


    }
    void RemoveWaypoint()
    {
        Waypoint selectedWaypoint =Selection.activeGameObject.GetComponent<Waypoint>();
       
        if(selectedWaypoint.nextWaypoint != null)
        {
            selectedWaypoint.nextWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
        }
        if(selectedWaypoint.previousWaypoint != null)
        {
            selectedWaypoint.previousWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
            Selection.activeGameObject = selectedWaypoint.previousWaypoint.gameObject;

            DestroyImmediate(selectedWaypoint.gameObject);
        }
    }
}
