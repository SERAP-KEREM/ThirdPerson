using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Item : MonoBehaviour
{

    [Header("General")]
    [SerializeField] private string _id = ""; public string id { get { return _id; } }
    private string _networkId = ""; public string networkID { get { return _networkId; } set { _networkId = value; } }

    private Rigidbody _rigidbody = null;
    private Collider _collider = null;

    private bool _canBePickedUp = false; public bool canBePickedUp { get { return _canBePickedUp; } set { _canBePickedUp = value; } }
    private bool _initialized = false;
    private bool _serverInitialized = false;

    private Vector3 _lastPosition = Vector3.zero;

    [System.Serializable]
    public struct Data
    {
        public string id;
        public string networkID;
        public int value;
        public float[] position;
        public float[] rotation;
    }

    public Data GetData()
    {
        Data data = new Data();
        data.id = id;
        data.networkID = networkID;
        data.value = GetAmount();

        data.position = new float[3];
        data.position[0] = transform.position.x;
        data.position[1] = transform.position.y;
        data.position[2] = transform.position.z;

        data.rotation = new float[3];
        data.rotation[0] = transform.eulerAngles.x;
        data.rotation[1] = transform.eulerAngles.y;
        data.rotation[2] = transform.eulerAngles.z;

        return data;
    }

    public virtual void Awake()
    {
        Initialize();
    }

    public virtual void Start()
    {
        if (transform.parent == null)
        {
            ServerInitialize();
            SetOnGroundStatus(true);
        }
    }

    public void ServerInitialize()
    {
        if (NetworkManager.Singleton.IsServer == false || _serverInitialized)
        {
            return;
        }
        _serverInitialized = true;
        if (GetType() == typeof(Weapon))
        {
            SetAmount(((Weapon)this).clipSize);
        }
        else
        {
            SetAmount(1);
        }
    }

    public virtual void Update()
    {
        if (_canBePickedUp && NetworkManager.Singleton.IsServer)
        {
            if (_lastPosition != transform.position)
            {
                SessionManager.singleton.UpdateItemPosition(this);
            }
            _lastPosition = transform.position;
        }
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _initialized = true;
        gameObject.tag = "Item";
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _collider.isTrigger = false;
        _rigidbody.mass = 40f;
    }

    public void SetOnGroundStatus(bool status)
    {
        _rigidbody.isKinematic = !status;
        _collider.enabled = status;
        _canBePickedUp = status;
        _lastPosition = transform.position;
    }

    public int GetAmount()
    {
        if (GetType() == typeof(Consumable)) { return ((Consumable)this).amount; }
        else if (GetType() == typeof(Ammo)) { return ((Ammo)this).amount; }
        else if (GetType() == typeof(Miscellaneous)) { return ((Miscellaneous)this).amount; }
        else if (GetType() == typeof(Weapon)) { return ((Weapon)this).ammo; }
        return 1;
    }

    public void SetAmount(int value)
    {
        if (GetType() == typeof(Consumable)) { ((Consumable)this).amount = value; }
        else if (GetType() == typeof(Ammo)) { ((Ammo)this).amount = value; }
        else if (GetType() == typeof(Miscellaneous)) { ((Miscellaneous)this).amount = value; }
        else if (GetType() == typeof(Weapon)) { ((Weapon)this).ammo = value; }
    }

    public void AddAmount(int value)
    {
        SetAmount(GetAmount() + value);
    }

}