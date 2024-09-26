using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Weapon : Item
{

    [Header("Settings")]
    [SerializeField] private Handle _type = Handle.TwoHanded; public Handle type { get { return _type; } }
    [SerializeField] private string _ammoID = ""; public string ammoID { get { return _ammoID; } }
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _fireRate = 0.2f;
    [SerializeField] private int _clipSize = 30; public int clipSize { get { return _clipSize; } }
    [SerializeField] private float _handKick = 5f; public float handKick { get { return _handKick; } }
    [SerializeField] private float _bodyKick = 5f; public float bodyKick { get { return _bodyKick; } }
    [SerializeField] public List<RigData> rigData = new List<RigData>();

    [Header("Referances")]
    [SerializeField] private Transform _muzzle = null;
    [SerializeField] private ParticleSystem _flash = null;

    [Header("Prefabs")]
    [SerializeField] private Projectile _projectile = null;


    public int maxAmmo = 30;
    public int currentAmmo;

    [System.Serializable]
    public class RigData
    {
        public string chatacterId;
        public Vector3 leftHandPosition = Vector3.zero;
        public Vector3 leftHandRotation = Vector3.zero;
        public Vector3 rightHandPosition = Vector3.zero;
        public Vector3 rightHandRotation = Vector3.zero;
    }
    public Weapon equippedWeapon;
   
    public enum Handle
    {
        OneHanded = 1, TwoHanded = 2
    }

    private float _fireTimer = 0;
    private int _ammo = 0; public int ammo { get { return _ammo; } set { _ammo = value; } }
    Vector3 target;
    public override void Awake()
    {
        base.Awake();
        _fireTimer += Time.realtimeSinceStartup;
    }
    private void Update()
    {
        

        if (equippedWeapon != null)
        {
            // Güvenli bir şekilde equippedWeapon'ın özelliklerine eriş
            int ammo = equippedWeapon.currentAmmo;
        }
        else
        {
            Debug.LogWarning("Weapon atanmamış!");
        }
    }
    public bool Shoot(Character character, Vector3 target)
    {
        float passedTime = Time.realtimeSinceStartup - _fireTimer;
        if (_ammo > 0 && passedTime >= _fireRate)
        {
            _ammo -= 1;
            _fireTimer = Time.realtimeSinceStartup;
            Projectile projectile = Instantiate(_projectile, _muzzle.position, Quaternion.identity);
            projectile.Initialize(character, target, _damage);
            if (_flash != null)
            {
                _flash.Play();
            }
            Ray ray = CameraManager1.mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
          float shootingRange = 100f;
            if (Physics.Raycast(ray, out hit, shootingRange))
            {
               // Debug.Log($"Ray bir objeye vurdu: {hit.collider.gameObject.name}");

                // Vurulan objenin tag'ini kontrol et
                if (hit.collider.CompareTag("Bodyguard"))
                {
                    Debug.Log($"Vurulan objenin tag'i: {hit.collider.tag}");
              
                    hit.collider.gameObject.GetComponent<Bodyguard>().TakeDamage(10);
                } if (hit.collider.CompareTag("NPC"))
                {
                    Debug.Log($"Vurulan objenin tag'i: {hit.collider.tag}");
              
                    hit.collider.gameObject.GetComponent<CharacterNavigatorScript>().CharacterHitDamage(10);
                }
            }
            return true;
        }
        return false;
    }
   
    public Vector3 LeftHandPosition(string chatacterId)
    {
        if (rigData != null)
        {
            for (int i = 0; i < rigData.Count; i++)
            {
                if (chatacterId == rigData[i].chatacterId)
                {
                    return rigData[i].leftHandPosition;
                }
            }
        }
        return Vector3.zero;
    }

    public Vector3 LeftHandRotation(string chatacterId)
    {
        if (rigData != null)
        {
            for (int i = 0; i < rigData.Count; i++)
            {
                if (chatacterId == rigData[i].chatacterId)
                {
                    return rigData[i].leftHandRotation;
                }
            }
        }
        return Vector3.zero;
    }

    public Vector3 RightHandPosition(string chatacterId)
    {
        if (rigData != null)
        {
            for (int i = 0; i < rigData.Count; i++)
            {
                if (chatacterId == rigData[i].chatacterId)
                {
                    return rigData[i].rightHandPosition;
                }
            }
        }
        return Vector3.zero;
    }

    public Vector3 RightHandRotation(string chatacterId)
    {
        if (rigData != null)
        {
            for (int i = 0; i < rigData.Count; i++)
            {
                if (chatacterId == rigData[i].chatacterId)
                {
                    return rigData[i].rightHandRotation;
                }
            }
        }
        return Vector3.zero;
    }

#if UNITY_EDITOR
    public void EditorApply(Weapon prefab)
    {
        if (Application.isPlaying && prefab != null && id == prefab.id)
        {
            Character character = transform.root.GetComponent<Character>();
            if (character != null && character.weapon == this)
            {
                RigManager rigManager = character.GetComponent<RigManager>();
                if (rigManager != null)
                {
                    List<RigData> rg = prefab.rigData;
                    int index = -1;
                    for (int i = 0; i < rg.Count; i++)
                    {
                        if (rg[i].chatacterId == character.id)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index < 0)
                    {
                        index = rg.Count;
                        rg.Add(new RigData());
                        rg[index].chatacterId = character.id;
                        if (rg.Count > 1)
                        {
                            int copyIndex = index - 1;
                            // ToDo: Copy similar data
                            rg[index].rightHandPosition = rg[copyIndex].rightHandPosition;
                            rg[index].rightHandRotation = rg[copyIndex].rightHandRotation;
                            rg[index].leftHandPosition = rg[copyIndex].leftHandPosition;
                            rg[index].leftHandRotation = rg[copyIndex].leftHandRotation;
                        }
                    }

                    rg[index].rightHandPosition = transform.localPosition;
                    rg[index].rightHandRotation = Functions.FixAngles(transform.localEulerAngles);
                    rg[index].leftHandPosition = rigManager.leftHandTarget.localPosition;
                    rg[index].leftHandRotation = Functions.FixAngles(rigManager.leftHandTarget.localEulerAngles);

                    rigData = rg;
                    prefab.rigData = rg;
                    UnityEditor.PrefabUtility.SavePrefabAsset(prefab.gameObject);
                }
            }
        }
    }
#endif

}