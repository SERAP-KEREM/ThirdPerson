using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class MafiaController : NetworkBehaviour
{
    public float runSpeed = 5f;
    public float detectionRange = 20f;
    public float escapeRange = 30f;
    public int maxHealth = 100;

    private int currentHealth;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Vector3 initialPosition;
    private bool isEscaping = false;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        initialPosition = transform.position;
        navMeshAgent.speed = runSpeed;
        currentHealth = maxHealth;

        if (IsServer)
        {
            NetworkObject.Spawn();
            Debug.Log($"MafiaController started at position {initialPosition}");
        }
    }

    void Update()
    {
        if (isDead) return;

        // NPC davranışları burada
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (isDead) return;
        TakeDamage(damage);
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }
}
