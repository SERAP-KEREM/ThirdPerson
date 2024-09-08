using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Functions : MonoBehaviour
{

    public static void SetLayerMask(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

    public static float FixAngle(float angle)
    {
        angle %= 360;
        return angle > 180 ? angle - 360 : angle < -180 ? angle + 360 : angle;
    }

    public static Vector3 FixAngles(Vector3 angles)
    {
        return new Vector3(FixAngle(angles.x), FixAngle(angles.y), FixAngle(angles.z));
    }

}