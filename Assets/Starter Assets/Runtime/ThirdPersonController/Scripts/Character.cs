using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.TextCore.Text;
using UnityEngine.Windows;


public class Character : MonoBehaviour
{
    public bool isLocalPlayer = false;
    [SerializeField] private string _id = ""; public string id {  get { return _id; } }
    [SerializeField] private Transform _weaponHolder = null;

    private Weapon _weapon = null; public Weapon weapon { get { return _weapon; } }
    private Ammo _ammo = null; public Ammo ammo { get { return _ammo; } }
    
    private List<Item> _items = new List<Item>();
    private Animator _animator=null;    
    private RigManager _rigManager = null;
    private Weapon _weaponToEquip = null;

    private bool _reloading = false; public bool reloading { get { return _reloading;} }
    private bool _switchingWeapon = false; public bool switchingWeapon { get { return _switchingWeapon; } }


    private Rigidbody[] _ragdollRigidbodies = null;
    private Collider[] _ragdollColliders= null;

    private float _health = 100;

    private bool _grounded = false; public bool isGrounded { get { return _grounded;} set { _grounded = value; } }
    private bool _walking = false; public bool walking { get { return _walking; } set { _walking = value; } }
    private float _speedAnimationMultiplier = 0f; public float speedAnimationMultiplier { get { return _speedAnimationMultiplier; } set { _speedAnimationMultiplier = value; } }
    private bool _aiming = false; public bool aiming { get { return _aiming; } set { _aiming = value; } }
    private bool _sprinting = false; public bool sprinting { get { return _sprinting; } set { _sprinting = value; } }


    private float _aimLayerWeight = 0;
    private Vector2 _aimedMovingAnimationsInput = Vector2.zero;
    private float aimRigWeight = 0f;
    private float leftHandWeight = 0f;

    private Vector3 _aimTarget = Vector3.zero; public Vector3 aimTarget { get { return _aimTarget; } set { _aimTarget = value; } }
    private Vector3 _lastPosition = Vector3.zero;

    private void Awake()
    {
        _ragdollColliders=GetComponentsInChildren<Collider>();
        _ragdollRigidbodies=GetComponentsInChildren<Rigidbody>();
        if(_ragdollRigidbodies!=null )
        {
            for(int i = 0;i< _ragdollRigidbodies.Length;i++)
            {
                _ragdollRigidbodies[i].mass *= 50; 
            }
        }

        if (_ragdollColliders != null)
        {
            for (int i = 0; i < _ragdollColliders.Length; i++)
            {
                _ragdollColliders[i].isTrigger = false;
            }
        }
        SetRagdollStatus(false);
        _rigManager = GetComponent<RigManager>();
        _animator = GetComponent<Animator>();
        Initialize(new Dictionary<string, int> { { "ScarL", 1 }, { "AKM", 1 },{ "7.62x39mm",1000 } } );
    }

    private void Start()
    {
        if(isLocalPlayer)
        {
            SetLayer(transform, LayerMask.NameToLayer("LocalPlayer"));

        }
        else
        {
            SetLayer(transform, LayerMask.NameToLayer("NetworkPlayer"));
        }
    }
    private void Update()
    {
        bool armed=weapon != null;

        _aimLayerWeight = Mathf.Lerp(_aimLayerWeight,_switchingWeapon || (armed && (_aiming || _reloading)) ? 1f : 0f, 10f * Time.deltaTime);
        _animator.SetLayerWeight(1, _aimLayerWeight);

        aimRigWeight = Mathf.Lerp(aimRigWeight, armed && _aiming && !_reloading ? 1f : 0f, 10f * Time.deltaTime);
        leftHandWeight = Mathf.Lerp(leftHandWeight-0.1f, armed && _switchingWeapon == false && !_reloading && (_aiming || (_grounded && _weapon.type == Weapon.Handle.TwoHanded)) ? 1f : 0f, 10f * Time.deltaTime);


        _rigManager.aimTarget = transform.position + transform.forward * 10f;
      //  _rigManager.aimTarget = _aimTarget;
        _rigManager.aimWeight = aimRigWeight;
        _rigManager.leftHandWeight = leftHandWeight;


        if (_sprinting)
        {
            speedAnimationMultiplier = 3;
        }
        else if (_walking)
        {
            speedAnimationMultiplier = 1;
        }
        else
        {
            speedAnimationMultiplier = 2;
        }
        Vector3 deltaPosition= transform.InverseTransformDirection(transform.position-_lastPosition).normalized;

        _aimedMovingAnimationsInput = Vector2.Lerp(_aimedMovingAnimationsInput, new Vector2(deltaPosition.x,deltaPosition.z) * _speedAnimationMultiplier, 10f * Time.deltaTime);
        _animator.SetFloat("Speed_X", _aimedMovingAnimationsInput.x);
        _animator.SetFloat("Speed_Y", _aimedMovingAnimationsInput.y);
    }

    private void LateUpdate()
    {
        _lastPosition = transform.position;
    }
    private void SetRagdollStatus(bool enabled)
    {
        if (_ragdollRigidbodies != null)
        {
            for (int i = 0; i < _ragdollRigidbodies.Length; i++)
            {
                _ragdollRigidbodies[i].isKinematic = !enabled;
            }
        }
    }

    public void Initialize(Dictionary<string, int> items)
    {
        if (items != null && PrefabManager.singleton != null)
        {
            int firstWeaponIndex = -1;
            foreach (var itemData in items)
            {
                Item prefab = PrefabManager.singleton.GetItemPrefab(itemData.Key);
                if (prefab != null && itemData.Value > 0)
                {
                    for (int i = 1; i <= itemData.Value; i++)
                    {
                        bool done= false;   
                        Item item = Instantiate(prefab, transform);
                        if (item.GetType() == typeof(Weapon))
                        {
                            Weapon w = (Weapon)item;
                            item.transform.SetParent(_weaponHolder);
                            item.transform.localPosition = w.rightHandPosition;
                            item.transform.localEulerAngles = w.rightHandRotation;

                            if (firstWeaponIndex < 0)
                            {
                                firstWeaponIndex = _items.Count;
                            }

                        }
                        else if(item.GetType() == typeof(Ammo))
                        {
                            Ammo a = (Ammo)item;
                            a.amount=itemData.Value;
                            done = true;

                        }
                        item.gameObject.SetActive(false);
                        _items.Add(item);
                        if(done)
                        {
                            break;
                        }
                    }
                }
            }
            if (firstWeaponIndex >= 0 && _weapon == null)
            {
                _weaponToEquip=(Weapon)_items[firstWeaponIndex];
                OnEquip();
            }
        }
    }

    public void ChangeWeapon(float direction)
    {
        Debug.Log("scroll");
        int x = direction > 0 ? 1 : direction < 0 ? -1 : 0;

        if (x != 0 && _switchingWeapon == false)
        {
            if(x>0)
            {
                NextWeapon();
            }
            else
            {
                PrevWeapon();
            }
         }
    }

    private void NextWeapon()
    {
        int first = -1;
        int current = -1;

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null && _items[i].GetType() == typeof(Weapon))
            {
                if (_weapon !=null && _items[i].gameObject == _weapon.gameObject)
                {
                    current = i;
                }
                else
                {
                    if (current >= 0 )
                    {
                        EquipWeapon((Weapon)_items[i]);
                        return;
                    }
                    else if (first<0)
                    {
                        first = i;
                    }
                }
            }
        }
        if (first >= 0)
        {
            EquipWeapon((Weapon)_items[first]);
        }
    }
    private void PrevWeapon()
    {
        int last = -1;
        int current = -1;

        for (int i = _items.Count - 1 ; i >= 0; i--)
        {
            if (_items[i] != null && _items[i].GetType() == typeof(Weapon))
            {
                if (_weapon != null && _items[i].gameObject == _weapon.gameObject)
                {
                    current = i;
                }
                else
                {
                    if (current >= 0)
                    {
                        EquipWeapon((Weapon)_items[i]);
                        return;
                    }
                    else if (last < 0)
                    {
                        last = i;
                    }
                }
            }
        }
        if (last >= 0)
        {
            EquipWeapon((Weapon)_items[last]);
        }
    }
    public void EquipWeapon(Weapon weapon)
    {
        if (_switchingWeapon || weapon==null)
        {
            return;
        }

    _weaponToEquip= weapon; 

        if(_weapon !=null)
        {
            HolsterWeapon();
        }
        else
        {
            _switchingWeapon = true;
            _animator.SetTrigger("Equip");
        }
    }
    
    private void _EquipWeapon()
    {
        if(_weaponToEquip != null)
        {
            _weapon = _weaponToEquip;
            _weaponToEquip = null;
            if (weapon.transform.parent != _weaponHolder)
            {
                _weapon.transform.SetParent(_weaponHolder);
                _weapon.transform.localPosition = _weapon.rightHandPosition;
                _weapon.transform.localEulerAngles = _weapon.rightHandRotation;
            }
            _rigManager.SetLeftHandGripData(_weapon.leftHandPosition, _weapon.LeftHandRotation);
            _weapon.gameObject.SetActive(true);

            _ammo = null;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].GetType() == typeof(Ammo) && _weapon.ammoID == _items[i].id)
                {
                    _ammo = (Ammo)_items[i];
                    break;
                }

            }
        }

       
    }

    public void OnEquip()
    {
        _EquipWeapon();
    }

    private void _HolsterWeapon()
    {
        if (_weapon != null)
        {
            _weapon.gameObject.SetActive(false);
            _weapon = null;
            _ammo= null;    
        }
    } 
    public void HolsterWeapon()
    {
        if(_switchingWeapon)
        {
            return;
        }
        if (_weapon != null)
        {
            _switchingWeapon = true;
            _animator.SetTrigger("Holster");
        }
    }

    public void OnHolster()
    {
        _HolsterWeapon();
        if(_weaponToEquip != null)
        {
            OnEquip();
        }
    }

    public void ApplyDamage(Character shooter, Transform hit,float damage)
    {
        if (_health > 0) 
        {
            _health -= damage;
            if(_health <= 0)
            {
                _health = 0;
                SetRagdollStatus(true);
                Destroy(_rigManager);
                Destroy(GetComponent<RigBuilder>());
                Destroy(_animator);

                ThirdPersonController thirdPersonController = GetComponent<ThirdPersonController>();
                if (thirdPersonController != null)
                {
                    Destroy(thirdPersonController);
                }

                CharacterController characterController = GetComponent<CharacterController>();
                if (characterController !=null)
                {
                    Destroy(characterController);
                    
                }
                Destroy(this);
            }
        }
    }

    public void Reload()
    {
        if(_weapon != null && !_reloading && _weapon.ammo<_weapon.clipSize && _ammo != null && _ammo.amount>0)
        {
            _animator.SetTrigger("Reload");
            _reloading = true;
        }
     
    }
    public void ReloadFinished()
    {
        
        if (_weapon != null && _weapon.ammo < _weapon.clipSize && _ammo != null && _ammo.amount > 0)
        {
            int amount = _weapon.clipSize - _weapon.ammo;
            if(_ammo.amount < amount)
            {
                amount = _ammo.amount;  
            }
            _ammo.amount -= amount;
            _weapon.ammo += amount;
        }
      
        _reloading = false;
    }


    public void HolsterFinished()
    {
        _switchingWeapon = false;
    }   
    public void EquipFinished()
    {
        _switchingWeapon = false;
    }

    private void SetLayer(Transform root, int layer)
    {
        var children  = root.GetComponentsInChildren<Transform>(true);
        foreach(var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
