using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public Item[] _items = null;
    private static PrefabManager _singleton = null;

    public static PrefabManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<PrefabManager>();
            }
            return _singleton;
        }
    }
    public Item GetItemPrefab(string id)
    {
        if (_items != null)
        {
            for(int i=0;i<_items.Length;i++)
            {
                if (_items[i] !=null && _items[i].id==id)
                {
                    return _items[i];
                }
            }
        }
        return null;
    }
}
