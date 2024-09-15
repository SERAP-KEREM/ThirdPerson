using LitJson;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Character : NetworkBehaviour
{

    [SerializeField] private string _id = ""; public string id { get { return _id; } }
    [SerializeField] private Transform _weaponHolder = null;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;


    private Weapon _weapon = null; public Weapon weapon { get { return _weapon; } }
   private Ammo _ammo = null; public Ammo ammo { get { return _ammo; } }
    int _ammoCount = 0;
     
   private CanvasManager _canvasManager = null; public CanvasManager canvasManager { get { return _canvasManager; } }

    private List<Item> _items = new List<Item>(); public List<Item> inventory { get { return _items; } }
    private Animator _animator = null;
    private RigManager _rigManager = null;
    private Weapon _weaponToEquip = null;
    private NetworkObject _networkObject = null;
    private bool _reloading = false; public bool reloading { get { return _reloading; } }
    private bool _switchingWeapon = false; public bool switchingWeapon { get { return _switchingWeapon; } }

    private Rigidbody[] _ragdollRigidbodies = null;
    private Collider[] _ragdollColliders = null;

    private float _health = 100; public float health { get { return _health; } }

    private bool _grounded = false; public bool isGrounded { get { return _grounded; } set { _grounded = value; } }
    private bool _walking = false; public bool walking { get { return _walking; } set { _walking = value; } }
    private float _speedAnimationMultiplier = 0; public float speedAnimationMultiplier { get { return _speedAnimationMultiplier; } }
    private bool _aiming = false; public bool aiming { get { return _aiming; } set { _aiming = value; } }
    private bool _sprinting = false; public bool sprinting { get { return _sprinting; } set { _sprinting = value; } }
    private float _aimLayerWieght = 0;
    private Vector2 _aimedMovingAnimationsInput = Vector2.zero;
    private float aimRigWieght = 0;
    private float leftHandWieght = 0;
    private Vector3 _aimTarget = Vector3.zero; public Vector3 aimTarget { get { return _aimTarget; } set { _aimTarget = value; } }
    private Vector3 _lastAimTarget = Vector3.zero;
    private Vector3 _lastPosition = Vector3.zero;

    private ulong _clientID = 0; public ulong clientID { get { return _clientID; } }
    private bool _initialized = false;
    private bool _componentsInitialized = false;

    private float _moveSpeed = 0; public float moveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
    private float _moveSpeedBlend = 0;
    private float _lastMoveSpeed = 0;

    private Vector2 _aimedMoveSpeed = Vector2.zero;
    private Vector2 _lastAimedMoveSpeed = Vector2.zero;
    private bool _lastAiming = false;

    public static Character localPlayer = null;


    public int maxHealth = 100;
    public int currentHealth = 100;
  
  

    [System.Serializable]
    public struct Data
    {
        public float health;
        public List<BaseData> items;
        public List<string> itemsId;
        public List<string> equippedIds;
    }

    [System.Serializable]
    public struct BaseData
    {
        public string id;
        public int count;
    }

    public Data GetData()
    {
        Data data = new Data();
        data.health = _health;
        data.items = new List<BaseData>();
        data.itemsId = new List<string>();
        data.equippedIds = new List<string>();
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] == null)
            {
                continue;
            }

            int value = _items[i].GetAmount();

            BaseData baseData = new BaseData();
            baseData.id = _items[i].id;
            baseData.count = value;

            data.items.Add(baseData);
            data.itemsId.Add(_items[i].networkID);

            if (_weaponToEquip != null)
            {
                if (_items[i] == _weaponToEquip)
                {
                    data.equippedIds.Add(_weaponToEquip.networkID);
                    for (int j = 0; j < _items.Count; j++)
                    {
                        if (_items[i] != null && _items[i].GetType() == typeof(Ammo) && _weaponToEquip.ammoID == _items[i].id)
                        {
                            data.equippedIds.Add(_items[i].networkID);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (_weapon != null && _items[i] == _weapon)
                {
                    data.equippedIds.Add(_items[i].networkID);
                }
                else if (_ammo != null && _items[i] == _ammo)
                {
                    data.equippedIds.Add(_items[i].networkID);
                }
            }
        }
        return data;
    }

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (_componentsInitialized)
        {
            return;
        }
        gameObject.tag = "Character";
        _componentsInitialized = true;
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        _ragdollColliders = GetComponentsInChildren<Collider>();
        if (_ragdollRigidbodies != null)
        {
            for (int i = 0; i < _ragdollRigidbodies.Length; i++)
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
        _fallTimeoutDelta = FallTimeout;
        _networkObject = GetComponent<NetworkObject>();
        _networkObject.DontDestroyWithOwner = false;
        currentHealth = maxHealth;

        _canvasManager = FindObjectOfType<CanvasManager>();


    }

    public void InitializeDummy(string weaponID)
    {
        InitializeComponents();
        if (_weapon != null)
        {
            Destroy(_weapon.gameObject);
            _weapon = null;
        }
        Item prefab = PrefabManager.singleton.GetItemPrefab(weaponID);
        if (prefab != null && prefab.GetType() == typeof(Weapon))
        {
            Item item = Instantiate(prefab, transform);
            item.Initialize();
            item.SetOnGroundStatus(false);
            Weapon w = (Weapon)item;
            item.transform.SetParent(_weaponHolder);
            item.transform.localPosition = w.RightHandPosition(_id);
            item.transform.localEulerAngles = w.RightHandRotation(_id);
            _rigManager.SetLeftHandGripData(w.LeftHandPosition(_id), w.LeftHandRotation(_id));
            _weapon = w;
        }
    }

    public void InitializeServer(List<BaseData> items, List<string> itemsId, List<string> equippedIds, ulong clientID)
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        InitializeComponents();
        _clientID = clientID;
        Functions.SetLayerMask(transform, LayerMask.NameToLayer("NetworkPlayer"));
        _Initialize(items, itemsId, equippedIds);
    }

    [ClientRpc]
    public void InitializeClientRpc(string itemsJson, string itemsIdJson, string equippedJson, string itemsOnGroundJson, ulong clientID)
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        InitializeComponents();
        _clientID = clientID;
        if (IsOwner)
        {
            Functions.SetLayerMask(transform, LayerMask.NameToLayer("LocalPlayer"));
            localPlayer = this;
        }
        else
        {
            Functions.SetLayerMask(transform, LayerMask.NameToLayer("NetworkPlayer"));
        }
        List<BaseData> items = JsonMapper.ToObject<List<BaseData>>(itemsJson);
        List<string> itemsId = JsonMapper.ToObject<List<string>>(itemsIdJson);
        List<string> equippedIds = JsonMapper.ToObject<List<string>>(equippedJson);
        List<Item.Data> itemsOnGround = JsonMapper.ToObject<List<Item.Data>>(itemsOnGroundJson);
        InitializeItemsOnGround(itemsOnGround);
        if (items != null && itemsId != null)
        {
            _Initialize(items, itemsId, equippedIds);
        }
    }

    private void InitializeItemsOnGround(List<Item.Data> itemsOnGround)
    {
        Item[] allItems = FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        List<Item> itemsOnGroundInScene = new List<Item>();
        if (allItems != null)
        {
            for (int i = 0; i < allItems.Length; i++)
            {
                if (allItems[i].transform.parent == null)
                {
                    itemsOnGroundInScene.Add(allItems[i]);
                }
            }
        }
        for (int i = 0; i < itemsOnGroundInScene.Count; i++)
        {
            bool matched = false;
            for (int j = 0; j < itemsOnGround.Count; j++)
            {
                if (itemsOnGroundInScene[i].id == itemsOnGround[j].id)
                {
                    itemsOnGroundInScene[i].networkID = itemsOnGround[j].networkID;
                    itemsOnGroundInScene[i].transform.position = new Vector3(itemsOnGround[j].position[0], itemsOnGround[j].position[1], itemsOnGround[j].position[2]);
                    itemsOnGroundInScene[i].transform.eulerAngles = new Vector3(itemsOnGround[j].rotation[0], itemsOnGround[j].rotation[1], itemsOnGround[j].rotation[2]);
                    itemsOnGroundInScene[i].SetAmount(itemsOnGround[j].value);
                    itemsOnGroundInScene[i].SetOnGroundStatus(true);
                    itemsOnGround.RemoveAt(j);
                    matched = true;
                    break;
                }
            }
            if (matched == false)
            {
                Destroy(itemsOnGroundInScene[i].gameObject);
            }
        }
        for (int i = 0; i < itemsOnGround.Count; i++)
        {
            Item prefab = PrefabManager.singleton.GetItemPrefab(itemsOnGround[i].id);
            if (prefab != null)
            {
                Item item = Instantiate(prefab);
                item.networkID = itemsOnGround[i].networkID;
                item.Initialize();
                item.SetOnGroundStatus(true);
                item.SetAmount(itemsOnGround[i].value);
                item.transform.position = new Vector3(itemsOnGround[i].position[0], itemsOnGround[i].position[1], itemsOnGround[i].position[2]);
                item.transform.eulerAngles = new Vector3(itemsOnGround[i].rotation[0], itemsOnGround[i].rotation[1], itemsOnGround[i].rotation[2]);
            }
        }
    }

    [ClientRpc]
    public void InitializeClientRpc(string dataJson, ulong clientID, ClientRpcParams rpcParams = default)
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        InitializeComponents();
        _clientID = clientID;
        if (IsOwner)
        {
            Functions.SetLayerMask(transform, LayerMask.NameToLayer("LocalPlayer"));
            localPlayer = this;
        }
        else
        {
            Functions.SetLayerMask(transform, LayerMask.NameToLayer("NetworkPlayer"));
        }
        Data data = JsonMapper.ToObject<Data>(dataJson);
        _health = data.health;
        _Initialize(data.items, data.itemsId, data.equippedIds);
        if (_health <= 0)
        {
            
            HealthCheck();
        }
    }
    private float _previousHealth;
    private int _previousAmmoCount;
    private void Update()
    {
        if (_health <= 0)
        {
            return;
        }
     
        if (Character.localPlayer != null && IsOwner)
        {
            
            // ServerRpc çağrısını sadece sahibi olan oyuncu yapabilir
            UpdatePlayerStatsServerRpc(_health, _ammoCount, NetworkManager.Singleton.LocalClientId);

            _canvasManager.UpdateAmmoText(_weapon.ammo, _ammoCount);

            if(_health > 0)
            {
                _canvasManager.UpdateHealthBar(_health);

            }
            else
            {
                _canvasManager.UpdateHealthBar(0);

            }
        }
       
        bool armed = _weapon != null;
        GroundedCheck();
        FreeFall();

        if (_shots.Count > 0 && !IsOwner)
        {
            if (_weapon != null && _weapon.networkID == _shots[0])
            {
                bool shoot = Shoot();
                if (shoot)
                {
                    _shots.RemoveAt(0);
                }
            }
            else
            {
                _shots.RemoveAt(0);
            }
        }

        _aimLayerWieght = Mathf.Lerp(_aimLayerWieght, _switchingWeapon || (armed && (_aiming || _reloading)) ? 1f : 0f, 10f * Time.deltaTime);
        _animator.SetLayerWeight(1, _aimLayerWieght);

        aimRigWieght = Mathf.Lerp(aimRigWieght, armed && _aiming && !_reloading ? 1f : 0f, 10f * Time.deltaTime);
        leftHandWieght = Mathf.Lerp(leftHandWieght, armed && _switchingWeapon == false && !_reloading && (_aiming || (_grounded && _weapon.type == Weapon.Handle.TwoHanded)) ? 1f : 0f, 10f * Time.deltaTime);

        _rigManager.aimTarget = _aimTarget;
        _rigManager.aimWeight = aimRigWieght;
        _rigManager.leftHandWeight = leftHandWieght;

        _moveSpeedBlend = Mathf.Lerp(_moveSpeedBlend, _moveSpeed, Time.deltaTime * 10f);
        if (_moveSpeedBlend < 0.01f)
        {
            _moveSpeedBlend = 0f;
        }

        if (_sprinting)
        {
            _speedAnimationMultiplier = 3;
        }
        else if (_walking)
        {
            _speedAnimationMultiplier = 1;
        }
        else
        {
            _speedAnimationMultiplier = 2;
        }

        if (IsOwner)
        {
            Vector3 deltaPosition = transform.InverseTransformDirection(transform.position - _lastPosition).normalized;
            _aimedMoveSpeed = new Vector2(deltaPosition.x, deltaPosition.z) * _speedAnimationMultiplier;
        }


        _aimedMovingAnimationsInput = Vector2.Lerp(_aimedMovingAnimationsInput, _aimedMoveSpeed, 10f * Time.deltaTime);
        _animator.SetFloat("Speed_X", _aimedMovingAnimationsInput.x);
        _animator.SetFloat("Speed_Y", _aimedMovingAnimationsInput.y);
        _animator.SetFloat("Armed", armed ? 1f : 0f);
        _animator.SetFloat("Aimed", _aiming ? 1f : 0f);
        _animator.SetFloat("Speed", _moveSpeedBlend);

        if (IsOwner)
        {
            if (_aiming != _lastAiming)
            {
                OnAimingChangedServerRpc(_aiming);
                _lastAiming = _aiming;
            }
            if (_aimTarget != _lastAimTarget)
            {
                OnAimTargetChangedServerRpc(_aimTarget);
                _lastAimTarget = _aimTarget;
            }
            if (_aiming)
            {
                if (_aimedMoveSpeed != _lastAimedMoveSpeed)
                {
                    OnAimingMoveChangedServerRpc(_aimedMoveSpeed);
                    _lastAimedMoveSpeed = _aimedMoveSpeed;
                }
            }
            else
            {
                if (_moveSpeed != _lastMoveSpeed)
                {
                    OnMoveSpeedChangedServerRpc(moveSpeed);
                    _lastMoveSpeed = _moveSpeed;
                }
            }
        }

    }



    [ServerRpc]
    public void UpdatePlayerStatsServerRpc(float newHealth, int totalAmmo, ulong clientId)
    {
        // Server'da oyuncu verilerini güncelle
        _health = newHealth;

        // Toplam mermiyi ve şarjörleri güncelle
        _ammoCount = totalAmmo % _weapon.clipSize;  // Şarjör başına kalan mermi sayısı
        _ammo.amount = totalAmmo / _weapon.clipSize;  // Toplam şarjör sayısı

        // Client'lara sağlık ve mühimmat bilgisini güncelle
        UpdateUIOnClientRpc(_health, _ammoCount, NetworkManager.Singleton.LocalClientId);
    }

    [ClientRpc]
   
    public void UpdateUIOnClientRpc(float health, int ammoCount, ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            // Bu client'a ait verileri güncelle
             _canvasManager.UpdateHealthBar(health);
            _canvasManager.UpdateAmmoText(_weapon.ammo, ammoCount);
        }
    }







    [ServerRpc]
    public void OnAimTargetChangedServerRpc(Vector3 value)
    {
        _aimTarget = value;
        OnAimTargetChangedClientRpc(value);
    }

    [ClientRpc]
    public void OnAimTargetChangedClientRpc(Vector3 value)
    {
        if (!IsOwner)
        {
            _aimTarget = value;
        }
    }

    [ServerRpc]
    public void OnAimingMoveChangedServerRpc(Vector2 value)
    {
        _aimedMoveSpeed = value;
        OnAimingMoveChangedClientRpc(value);
    }

    [ClientRpc]
    public void OnAimingMoveChangedClientRpc(Vector2 value)
    {
        if (!IsOwner)
        {
            _aimedMoveSpeed = value;
        }
    }

    [ServerRpc]
    public void OnAimingChangedServerRpc(bool value)
    {
        _aiming = value;
        OnAimingChangedClientRpc(value);
    }

    [ClientRpc]
    public void OnAimingChangedClientRpc(bool value)
    {
        if (!IsOwner)
        {
            _aiming = value;
        }
    }

    [ServerRpc]
    public void OnMoveSpeedChangedServerRpc(float value)
    {
        _moveSpeed = value;
        OnMoveSpeedChangedClientRpc(value);
    }

    [ClientRpc]
    public void OnMoveSpeedChangedClientRpc(float value)
    {
        if (!IsOwner)
        {
            _moveSpeed = value;
        }
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

    private void _Initialize(List<BaseData> items, List<string> itemsId, List<string> equippedIds)
    {
        InitializeComponents();
        if (items != null && PrefabManager.singleton != null)
        {
            int i = 0;
            int equippedWeaponIndex = -1;
            int equippedAmmoIndex = -1;
            foreach (var itemData in items)
            {
                Item prefab = PrefabManager.singleton.GetItemPrefab(itemData.id);
                if (prefab != null)
                {
                    Item item = Instantiate(prefab, transform);
                    item.Initialize();
                    item.SetOnGroundStatus(false);
                    item.networkID = itemsId[i];
                    item.SetAmount(itemData.count);
                    if (item.GetType() == typeof(Weapon))
                    {
                        Weapon w = (Weapon)item;
                        item.transform.SetParent(_weaponHolder);
                        item.transform.localPosition = w.RightHandPosition(_id);
                        item.transform.localEulerAngles = w.RightHandRotation(_id);
                        if (equippedIds.Contains(item.networkID) || equippedWeaponIndex < 0)
                        {
                            equippedWeaponIndex = i;
                        }
                    }
                    else if (item.GetType() == typeof(Ammo))
                    {
                        Ammo a = (Ammo)item;
                        if (equippedIds.Contains(item.networkID))
                        {
                            equippedAmmoIndex = i;
                        }
                    }

                    item.gameObject.SetActive(false);
                    _items.Add(item);
                    i++;
                 
                   
                }
            }
            if (_health > 0)
            {
                if (equippedWeaponIndex >= 0 && _weapon == null)
                {
                    _weaponToEquip = (Weapon)_items[equippedWeaponIndex];
                    OnEquip();
                }
                if (equippedAmmoIndex >= 0)
                {
                    _EquipAmmo((Ammo)_items[equippedAmmoIndex]);
                }

                if (_ammo != null && _ammo.amount > 0 && _weapon.ammo < _weapon.clipSize)
                {
                    // Şarjörü doldurmak için gereken mermi miktarını hesapla
                    int amount = _weapon.clipSize - _weapon.ammo;

                    // Eğer mevcut mermi miktarı, gereken miktardan azsa, sadece mevcut mermileri kullan
                    if (_ammo.amount < amount)
                    {
                        amount = _ammo.amount;
                    }

                    // Mevcut mermi miktarını azalt
                    _ammo.amount -= amount;

                    // Şarjörü güncelle (silahın mevcut mermi miktarına gerekli mermi miktarını ekle)
                    _weapon.ammo += amount;

                }

            }
        }
    }

    public void ChangeWeapon(float direction)
    {
        int x = direction > 0 ? 1 : direction < 0 ? -1 : 0;
        if (x != 0 && _switchingWeapon == false)
        {
            if (x > 0)
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
                    else if (first < 0)
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
        for (int i = _items.Count - 1; i >= 0; i--)
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
        if (_switchingWeapon || weapon == null)
        {
            return;
        }
        if (IsOwner)
        {
            EquipWeaponServerRpc(weapon.networkID);
        }
        _weaponToEquip = weapon;
        if (_weapon != null)
        {
            HolsterWeapon();
        }
        else
        {
            _switchingWeapon = true;
            _animator.SetTrigger("Equip");
        }
    }

    [ServerRpc]
    public void EquipWeaponServerRpc(string networkID)
    {
        EquipWeaponSync(networkID);
        EquipWeaponClientRpc(networkID);
    }

    [ClientRpc]
    public void EquipWeaponClientRpc(string networkID)
    {
        if (!IsOwner)
        {
            EquipWeaponSync(networkID);
        }
    }

    private void EquipWeaponSync(string networkID)
    {
        Weapon weapon = null;
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i] != null && _items[i].networkID == networkID && _items[i].GetType() == typeof(Weapon))
            {
                weapon = (Weapon)_items[i];
                break;
            }
        }
        if (weapon != null)
        {
            EquipWeapon(weapon);
        }
        else
        {
            // Problem
        }
    }

    private void _EquipWeapon()
    {
        if (_weaponToEquip != null)
        {
            _weapon = _weaponToEquip;
            _weaponToEquip = null;
            if (_weapon.transform.parent != _weaponHolder)
            {
                _weapon.transform.SetParent(_weaponHolder);
                _weapon.transform.localPosition = _weapon.RightHandPosition(_id);
                _weapon.transform.localEulerAngles = _weapon.RightHandRotation(_id);
            }
            _rigManager.SetLeftHandGripData(_weapon.LeftHandPosition(_id), _weapon.LeftHandRotation(_id));
            _weapon.gameObject.SetActive(true);
            _ammo = null;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null && _items[i].GetType() == typeof(Ammo) && _weapon.ammoID == _items[i].id)
                {
                    _EquipAmmo((Ammo)_items[i]);
                    break;
                }
            }
        }
    }

    private void _EquipAmmo(Ammo ammo)
    {
        if (ammo != null)
        {
            if (_weapon != null && _weapon.ammoID != ammo.id)
            {
                return;
            }
            _ammo = ammo;
            if (_ammo.transform.parent != transform)
            {
                _ammo.transform.SetParent(transform);
                _ammo.transform.localPosition = Vector3.zero;
                _ammo.transform.localEulerAngles = Vector3.zero;
                _ammo.gameObject.SetActive(false);
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
            _ammo = null;
        }
    }

    public void HolsterWeapon()
    {
        if (_switchingWeapon)
        {
            return;
        }
        if (_weapon != null)
        {
            if (IsOwner)
            {
                HolsterWeaponServerRpc(_weapon.networkID);
            }
            _switchingWeapon = true;
            _animator.SetTrigger("Holster");
        }
    }

    [ServerRpc]
    public void HolsterWeaponServerRpc(string weaponID)
    {
        HolsterWeaponSync(weaponID);
        HolsterWeaponClientRpc(weaponID);
    }

    [ClientRpc]
    public void HolsterWeaponClientRpc(string weaponID)
    {
        if (!IsOwner)
        {
            HolsterWeaponSync(weaponID);
        }
    }

    public void HolsterWeaponSync(string weaponID)
    {
        if (_weapon != null && _weapon.networkID == weaponID)
        {
            HolsterWeapon();
        }
        else
        {
            // Problem
        }
    }

    public void OnHolster()
    {
        _HolsterWeapon();
        if (_weaponToEquip != null)
        {
            OnEquip();
        }
    }

    public void ApplyDamage(Character shooter, Transform hit, float damage)
    {
        if (_health > 0 && damage > 0)
        {
            if (hit == _animator.GetBoneTransform(HumanBodyBones.Head))
            {
                damage *= 3f;
            }
            _health -= damage;
            if (_health <= 0)
            {
               
                _networkObject.DontDestroyWithOwner = true;
            }
            HealthCheck();
            ApplyDamageClientRpc(shooter.clientID, clientID, damage, _health);
        }
    }

    [ClientRpc]
    private void ApplyDamageClientRpc(ulong shooter, ulong target, float damage, float remainedHealth)
    {
        _health = remainedHealth;
        HealthCheck();
    }

    private void HealthCheck()
    {
        if (_health <= 0)
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
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                Destroy(controller);
            }

            if (_weapon != null)
            {
                _items.Remove(_weapon);
                _weapon.transform.SetParent(null, true);
                _weapon.SetOnGroundStatus(true);
            }

            ClientNetworkTransform networkTransform = GetComponent<ClientNetworkTransform>();
            if (networkTransform != null)
            {
                networkTransform.SyncPositionX = false;
                networkTransform.SyncPositionY = false;
                networkTransform.SyncPositionZ = false;
                networkTransform.SyncRotAngleX = false;
                networkTransform.SyncRotAngleY = false;
                networkTransform.SyncRotAngleZ = false;
            }
        }
    }



    public void Reload()
    {
        if (_weapon != null && !_reloading && _weapon.ammo < _weapon.clipSize && _ammo != null && _ammo.amount > 0)
        {
            if (IsOwner)
            {
                ReloadServerRpc(weapon.networkID, _ammo.networkID);
            }
            _animator.SetTrigger("Reload");
            _reloading = true;
        }
    }






    [ServerRpc]
    public void ReloadServerRpc(string weaponID, string ammoID)
    {
        ReloadSync(weaponID, ammoID);
        ReloadClientRpc(weaponID, ammoID);
    }

    [ClientRpc]
    public void ReloadClientRpc(string weaponID, string ammoID)
    {
        if (!IsOwner)
        {
            ReloadSync(weaponID, ammoID);
        }
    }

    private void ReloadSync(string weaponID, string ammoID)
    {
        if (_weapon != null && _ammo != null && _weapon.networkID == weaponID && _ammo.networkID == ammoID)
        {
            ReloadFinished();
        }
        else
        {
            // Problem - Weapon veya Ammo eşleşmediğinde buraya hata logu ekleyebilirsin.
        }
    }



    public void ReloadFinished()
    {
        if (_weapon != null && _weapon.ammo < _weapon.clipSize )
        {
            // Şarjörü doldurmak için gereken mermi miktarını hesapla
            int ammoNeeded = _weapon.clipSize - _weapon.ammo;

            // Elimizdeki toplam mermi miktarını hesapla
            int totalAmmo = _ammo.amount * _weapon.clipSize + _ammoCount;

           

            // Eğer toplam mermi şarjörü dolduracak kadar fazlaysa
            if (totalAmmo >= ammoNeeded)
            {
                // Şarjörü tamamen doldur
                _weapon.ammo = _weapon.clipSize;

                // Toplam mermiden kullanılan mermi miktarını düş
                totalAmmo -= ammoNeeded;

                // Kalan mermileri ve şarjör miktarını güncelle
                _ammoCount -= ammoNeeded;  // Kalan mermi
                _ammo.amount = _ammoCount / _weapon.clipSize;  // Kalan tam şarjör sayısı

             
            }
            else
            {
                // Yeterli mermi yoksa tüm kalan mermileri şarjöre ekle
                _weapon.ammo += totalAmmo;  // Şarjörü mevcut mermilerle doldur
                _ammoCount = 0;
                _ammo.amount = 0;

             
            }

            // Reload işlemi tamamlandı
          
            _reloading = false;

            // UI'yı güncellemek için ammo text fonksiyonunu çağır
            _canvasManager.UpdateAmmoText(_weapon.ammo, _ammoCount);
        }
    }



    public void HolsterFinished()
    {
        _switchingWeapon = false;
    }

    public void EquipFinished()
    {
        _switchingWeapon = false;
    }

    private float _fallTimeoutDelta;

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        _animator.SetBool("Grounded", _grounded);
    }

    private void FreeFall()
    {
        if (_grounded)
        {
            _fallTimeoutDelta = FallTimeout;
            _animator.SetBool("FreeFall", false);

        }
        else
        {
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _animator.SetBool("FreeFall", true);
            }
        }
    }

    public void Jump()
    {
        _animator.SetTrigger("Jump");
        JumpServerRpc();
    }

    [ServerRpc]
    public void JumpServerRpc()
    {
        _animator.SetTrigger("Jump");
        JumpClientRpc();
    }

    [ClientRpc]
    public void JumpClientRpc()
    {
        if (!IsOwner)
        {
            _animator.SetTrigger("Jump");
        }
    }

    private List<string> _shots = new List<string>();
    //-----
    public bool Shoot()
    {
        if (_weapon != null && !reloading && _aiming && _weapon.Shoot(this, _aimTarget))
        {
            if (IsOwner)
            {
                ShootServerRpc(_weapon.networkID);
            }
            _rigManager.ApplyWeaponKick(_weapon.handKick, _weapon.bodyKick);
            return true;
        }
        return false;
    }

    [ServerRpc]
    public void ShootServerRpc(string weaponID)
    {
        ShootSync(weaponID);
        ShootClientRpc(weaponID);
    }

    [ClientRpc]
    public void ShootClientRpc(string weaponID)
    {
        if (!IsOwner)
        {
            ShootSync(weaponID);
        }
    }

    public void ShootSync(string weaponID)
    {
        if (_weapon != null && _weapon.networkID == weaponID)
        {
            bool shoot = Shoot();
            if (!shoot)
            {
                _shots.Add(weaponID);
            }
        }
        else
        {
            // Problem
        }
    }



    /// ------

    [ServerRpc]
    public void UpdateAmmoServerRpc(ulong playerID, int newAmmoCount)
    {
        // Mermi bilgisini sunucuda güncelle
        _ammoCount = newAmmoCount;

        // Diğer istemcilere mermi bilgisini güncelleme çağrısı gönder
        UpdateAmmoClientRpc(playerID, newAmmoCount);
    }

    [ClientRpc]
    private void UpdateAmmoClientRpc(ulong playerID, int newAmmoCount)
    {
        if (NetworkManager.Singleton.LocalClientId == playerID) // Yalnızca ateş eden oyuncunun mermi bilgileri güncellenir
        {
            _ammoCount = newAmmoCount;
            _canvasManager.UpdateAmmoText(_weapon.ammo, _ammoCount);
        }
    }

    private bool _pickingItem = false;

    public void PickupItem(string networkID)
    {
        if (_pickingItem)
        {
            return;
        }
        _pickingItem = true;
        PickupItemServerRpc(networkID);
    }

    [ServerRpc]
    private void PickupItemServerRpc(string networkID, ServerRpcParams serverRpcParams = default)
    {
        bool success = false;
        Item[] allItems = FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Item merge = null;
        if (allItems != null)
        {
            for (int i = 0; i < allItems.Length; i++)
            {
                if (allItems[i].transform.parent == null && allItems[i].networkID == networkID)
                {
                    if (allItems[i].GetType() == typeof(Ammo))
                    {
                        for (int j = 0; j < _items.Count; j++)
                        {
                            if (_items[j].id == allItems[i].id)
                            {
                                merge = _items[j];
                                break;
                            }
                        }
                    }
                    AddItemToInventoryLocally(allItems[i], merge);
                    success = true;
                    break;
                }
            }
        }
        if (success)
        {
            PickupItemClientRpc(networkID, true, merge != null ? merge.networkID : "");
        }
        else
        {
            ulong[] target = new ulong[1];
            target[0] = serverRpcParams.Receive.SenderClientId;
            ClientRpcParams clientRpcParams = default;
            clientRpcParams.Send.TargetClientIds = target;
            PickupItemClientRpc(networkID, false, "", clientRpcParams);
        }
    }

    [ClientRpc]
    private void PickupItemClientRpc(string networkID, bool success, string mergeNetworkID, ClientRpcParams rpcParams = default)
    {
        if (success)
        {
            bool found = false;
            Item[] allItems = FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (allItems != null)
            {
                for (int i = 0; i < allItems.Length; i++)
                {
                    if (allItems[i].transform.parent == null && allItems[i].networkID == networkID)
                    {
                        found = true;
                        Item merge = null;
                        if (string.IsNullOrEmpty(mergeNetworkID) == false)
                        {
                            for (int j = 0; j < _items.Count; j++)
                            {
                                if (_items[j].networkID == mergeNetworkID)
                                {
                                    merge = _items[j];
                                    break;
                                }
                            }
                        }
                        AddItemToInventoryLocally(allItems[i], merge);
                        break;
                    }
                }
            }
            if (found == false)
            {
                // Problem
            }
        }
        _pickingItem = false;
    }

    public void AddItemToInventoryLocally(Item item, Item merge = null)
    {
        if (item == null || _items.Contains(item))
        {
            return;
        }

        if (merge != null && _items.Contains(merge))
        {
            if (merge.GetType() == item.GetType())
            {
                // Tür dönüşümünü güvenli şekilde yapın
                if (item is Ammo ammoItem && merge is Ammo ammoMerge)
                {
                    ammoMerge.amount += ammoItem.amount;
                    _ammoCount += weapon.clipSize * ammoItem.amount;
                    Destroy(item.gameObject);
                }
                else
                {
                    Debug.LogError("Merge and item are not of type Ammo.");
                }
            }
            else
            {
                Debug.LogError("Item and merge are of different types.");
            }
        }
        else
        {
            item.transform.SetParent(transform);
            item.Initialize();
            item.SetOnGroundStatus(false);
            if (item.GetType() == typeof(Weapon))
            {
                Weapon w = (Weapon)item;
                item.transform.SetParent(_weaponHolder);
                item.transform.localPosition = w.RightHandPosition(_id);
                item.transform.localEulerAngles = w.RightHandRotation(_id);
            }
            else if (item.GetType() == typeof(Ammo))
            {
                if (_ammo == null && _weapon != null && _weapon.ammoID == ((Ammo)item).id)
                {
                    _EquipAmmo((Ammo)item);
                }
                _ammoCount += weapon.clipSize * ((Ammo)item).amount;
            }
            item.gameObject.SetActive(false);
            _items.Add(item);
        }
    }


    public void RemoveItemFromInventoryLocally(Item item)
    {
        if (item == null || _items.Contains(item) == false)
        {
            return;
        }
        if (item == _weapon)
        {
            _weapon = null;
        }
        if (item == _ammo)
        {
            _ammo = null;
        }
        _items.Remove(item);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {/*
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }*/
    }

    private void OnLand(AnimationEvent animationEvent)
    {/*
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }*/
    }

    public void DropItem(Item item, int count)
    {
        if (item != null)
        {
            Dictionary<Item, int> items = new Dictionary<Item, int>();
            items.Add(item, count);
            DropItems(items);
        }
    }

    public void DropItems(Dictionary<Item, int> items)
    {
        Dictionary<string, int> serializableItems = new Dictionary<string, int>();
        foreach (var item in items)
        {
            if (item.Value <= 0 && item.Key.GetType() == typeof(Ammo))
            {
                continue;
            }
            if (item.Key != null && _items.Contains(item.Key))
            {
                serializableItems.Add(item.Key.networkID, item.Value);
            }
        }
        if (serializableItems.Count > 0)
        {
            string itemsJson = JsonMapper.ToJson(serializableItems);
            DropItemsServerRpc(itemsJson);
        }
    }

    [ServerRpc]
    private void DropItemsServerRpc(string itemsJson, ServerRpcParams serverRpcParams = default)
    {
        Dictionary<string, int> items = JsonMapper.ToObject<Dictionary<string, int>>(itemsJson);
        Dictionary<string, int> droppedItems = new Dictionary<string, int>();
        Dictionary<string, (string, int)> splitItems = new Dictionary<string, (string, int)>();
        foreach (var item in items)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (item.Key == _items[i].networkID)
                {
                    int count = item.Value;
                    int remained = 0;
                    int c = 0;
                    if (_items[i].GetType() == typeof(Weapon))
                    {
                        count = ((Weapon)_items[i]).ammo;
                    }
                    else
                    {
                        c = _items[i].GetAmount();
                        if (count <= 0)
                        {
                            break;
                        }
                        else if (c < count)
                        {
                            count = c;
                        }
                        else if (c > count)
                        {
                            remained = c - count;
                            c = count;
                            _items[i].SetAmount(c);
                        }
                    }
                    if (remained > 0)
                    {
                        Item prefab = PrefabManager.singleton.GetItemPrefab(_items[i].id);
                        if (prefab != null)
                        {
                            Item splitItem = Instantiate(prefab, transform);
                            splitItem.networkID = System.Guid.NewGuid().ToString();
                            splitItem.SetAmount(remained);
                            AddItemToInventoryLocally(splitItem);
                            splitItems.Add(splitItem.networkID, (_items[i].id, remained));
                        }
                        else
                        {
                            break;
                        }
                    }
                    _DropItem(_items[i]);
                    droppedItems.Add(item.Key, count);
                    break;
                }
            }
        }
        if (droppedItems.Count > 0)
        {
            string droppedItemsJson = JsonMapper.ToJson(droppedItems);
            string splitItemsJson = JsonMapper.ToJson(splitItems);
            DropItemsClientRpc(droppedItemsJson, splitItemsJson);
        }
    }

    [ClientRpc]
    private void DropItemsClientRpc(string droppedItemsJson, string splitItemsJson, ClientRpcParams serverRpcParams = default)
    {
        Dictionary<string, int> items = JsonMapper.ToObject<Dictionary<string, int>>(droppedItemsJson);
        Dictionary<string, (string, int)> splitItems = JsonMapper.ToObject<Dictionary<string, (string, int)>>(splitItemsJson);
        foreach (var item in items)
        {
            bool found = false;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].networkID == item.Key)
                {
                    _items[i].SetAmount(item.Value);
                    _DropItem(_items[i]);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                // Problem
            }
        }
        foreach (var item in splitItems)
        {
            Item prefab = PrefabManager.singleton.GetItemPrefab(item.Value.Item1);
            if (prefab != null)
            {
                Item splitItem = Instantiate(prefab, transform);
                splitItem.networkID = item.Key;
                splitItem.SetAmount(item.Value.Item2);
                AddItemToInventoryLocally(splitItem);
            }
        }
    }

    private void _DropItem(Item item)
    {
        if (_items.Contains(item) == false)
        {
            return;
        }
        if (item == _weapon)
        {
            _weapon = null;
        }
        if (item == _ammo)
        {
            _ammo = null;
        }
        item.transform.SetParent(null);
        item.SetOnGroundStatus(true);
        Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), 0, UnityEngine.Random.Range(-0.1f, 0.1f));
        item.transform.position = transform.position + transform.forward.normalized + Vector3.up + offset;
        item.transform.rotation = Quaternion.identity;
        item.gameObject.SetActive(true);
        _items.Remove(item);
    }

}