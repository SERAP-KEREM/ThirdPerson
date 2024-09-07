using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public Item[] _items = null;
    [SerializeField] private Character[] _charactors = null;

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
    public Character GetCharacterPrefab(string id)
    {
        if (_charactors != null)
        {
            for (int i = 0; i < _charactors.Length; i++)
            {
                if (_charactors[i] != null && _charactors[i].id == id)
                {
                    return _charactors[i];
                }
            }
        }
        return null;
    }
}
