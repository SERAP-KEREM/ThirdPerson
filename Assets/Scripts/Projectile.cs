using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    [SerializeField] private float _speed = 20;

    [Header("Prefabs")]
    [SerializeField] private Transform _defaultImpact = null;

    private float _damage = 1f;
    private bool _initialized = false;
    private Character _shooter = null;
    private Rigidbody _rigidbody = null;
    private Collider _collider = null;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) { return; }
        _initialized = true;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        _rigidbody.useGravity = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _collider = gameObject.GetComponent<Collider>();
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<SphereCollider>();
        }
        _collider.isTrigger = false;
        _collider.tag = "Projectile";
    }

    public void Initialize(Character shooter, Vector3 target, float damage)
    {
        Initialize();
        _shooter = shooter;
        _damage = damage;
        transform.LookAt(target);
        _rigidbody.velocity = transform.forward.normalized * _speed;
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((_shooter != null && collision.transform.root == _shooter.transform.root) || collision.gameObject.tag == "Projectile")
        {
            Physics.IgnoreCollision(collision.collider, _collider);
            return;
        }

        Character character = collision.transform.root.GetComponent<Character>();
        if (NetworkManager.Singleton.IsServer)
        {
            if (character != null)
            {
                character.ApplyDamage(_shooter, collision.transform, _damage);
            }
        }
        else
        {
            if (character != null)
            {

            }
            else if (_defaultImpact != null)
            {
                if (collision.gameObject.layer != LayerMask.NameToLayer("LocalPlayer") && collision.gameObject.layer != LayerMask.NameToLayer("NetworkPlayer"))
                {
                    Transform impact = Instantiate(_defaultImpact, collision.contacts[0].point, Quaternion.FromToRotation(Vector3.up, collision.contacts[0].normal));
                    Destroy(impact.gameObject, 30f);
                }
            }
        }

        Destroy(gameObject);
    }

}