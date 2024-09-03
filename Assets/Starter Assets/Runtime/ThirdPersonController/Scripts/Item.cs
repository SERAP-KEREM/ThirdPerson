using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private string _id = ""; public string id { get { return _id; } }
    private string _networId = ""; public string networkID { get { return _networId; } set { _networId = value; } }

    private Rigidbody _rigidbody = null;
    private Collider _collider = null;

    private bool _canBePickedUp = false; public bool canBePickedUp { get { return _canBePickedUp; } set { _canBePickedUp = value; } }
    private bool _initialized = false;


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

        if (this is Weapon)
        {
            data.value = ((Weapon)this).ammo;

        }
        else if (this is Ammo)
        {
            data.value = ((Ammo)this).amount;
        }

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
        if(transform.parent == null)
        {
            SetOnGroundStatus(true);
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
    }
}
