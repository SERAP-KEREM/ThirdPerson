using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tools : MonoBehaviour
{

    public static void SetLayerMask(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }

}