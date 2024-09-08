using Unity.VisualScripting;
using UnityEditor;

[CustomEditor(typeof(Weapon))] public class WeaponEditor : Editor
{

    private Weapon _weapon = null;

    private void OnEnable()
    {
        _weapon = null;
        string[] guids = AssetDatabase.FindAssets("t:prefab");
        Weapon weapon = target as Weapon;
        if (guids.Length > 0)
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                Weapon w = obj.GetComponent<Weapon>();
                if (w != null && weapon.id == w.id)
                {
                    _weapon = w;
                    break;
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Weapon weapon = target as Weapon;
        weapon.EditorApply(_weapon);
    }

}

[CustomEditor(typeof(EditorTarget))] public class EditorTargetManager : Editor
{

    private Weapon _weapon = null;

    private void OnEnable()
    {
        EditorTarget rig = target as EditorTarget;
        _weapon = null;
        string[] guids = AssetDatabase.FindAssets("t:prefab");
        if (rig.character != null && rig.character.weapon != null)
        {
            if (guids.Length > 0)
            {
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    Weapon w = obj.GetComponent<Weapon>();
                    if (w != null && rig.character.weapon.id == w.id)
                    {
                        _weapon = w;
                        break;
                    }
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorTarget rig = target as EditorTarget;
        if (rig.character != null && rig.character.weapon != null)
        {
            rig.character.weapon.EditorApply(_weapon);
        }
    }

}