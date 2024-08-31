using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class Character : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder = null;

    private Weapon _weapon = null; public Weapon weapon { get { return _weapon; } }
    private Ammo _ammo = null; public Ammo ammo { get { return _ammo; } }
    
    private List<Item> _items = new List<Item>();
    private Animator _animator=null;    
    private RigManager _rigManager = null;
    private Weapon _weaponToEquip = null;

    private bool _reloading = false; public bool reloading { get { return _reloading;} }
    private bool _switchWeapon = false; public bool switchWeapon { get { return _switchWeapon; } }


    private void Awake()
    {
        _rigManager = GetComponent<RigManager>();
        _animator = GetComponent<Animator>();
        Initialize(new Dictionary<string, int> { { "ScarL", 1 }, { "AKM", 1 },{ "7.62x39mm",1000 } } );
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

        if (x != 0 && _switchWeapon == false)
        {
            int before = -1;
            int current = -1;
            int after = -1;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].GetType() == typeof(Weapon))
                {
                    if (_items[i].gameObject == _weapon.gameObject)
                    {
                        current = i;
                    }
                    else
                    {
                        if (current < 0 && before < 0)
                        {
                            before = i;
                        }
                        if (current >= 0 && after < 0)
                        {
                            after = i;
                        }
                    }
                }
            }
            int target = -1;
            if (x > 0)
            {
                if (after >= 0)
                {
                    target = after;
                }


                else if (before >= 0)
                {
                    target = before;
                }
            }

            else
            {

                if (before >= 0)
                {
                    target = before;
                }
                else if (after >= 0)
                {
                    target = after;
                }
            }

        
             if (target >= 0)
             {
                EquipWeapon((Weapon)_items[target]);
             }
         }
    }
    public void EquipWeapon(Weapon weapon)
    {
        if (_switchWeapon || weapon==null)
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
            _switchWeapon = true;
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
            _rigManager.SetLeftHandGripData(_weapon.leftHandPosition, _weapon.leftHandPosition);
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
        if(_switchWeapon)
        {
            return;
        }
        if (_weapon != null)
        {
            _switchWeapon = true;
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

    }   
    public void EquipFinished()
    {
        _switchWeapon = false;
    }

}
