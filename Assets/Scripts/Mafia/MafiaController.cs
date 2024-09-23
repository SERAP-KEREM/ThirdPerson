using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class MafiaController : NetworkBehaviour
{
    [SerializeField] Character player; // Oyuncu referansı
    public float runSpeed = 5f;
    public float detectionRange = 20f;
    public float escapeRange = 30f;
    public float returnRange = 50f;
    public int maxHealth = 100;

    private int currentHealth;
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Vector3 initialPosition;
    private bool isEscaping = false;
    private bool isReturning = false;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<Character>();
    }

    void Start()
    {
        initialPosition = transform.position;
        navMeshAgent.speed = runSpeed;
        currentHealth = maxHealth;
        if (IsServer)
        {
            NetworkObject.Spawn();
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Character>();
            if (player == null) return;
        }

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (isDead) return;

        if (isEscaping)
        {
            if (distanceToPlayer >= escapeRange)
            {
                isEscaping = false;
                isReturning = true;
            }
            return;
        }

        if (isReturning)
        {
            navMeshAgent.SetDestination(initialPosition);
            if (Vector3.Distance(transform.position, initialPosition) <= 1f)
            {
                navMeshAgent.isStopped = true;
                animator.SetBool("Run", false);
                isReturning = false;
            }
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            // NPC burada başka bir işlem yapabilir.
        }
        else if (distanceToPlayer > returnRange)
        {
            isReturning = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            EscapeFromPlayer();
        }
    }

    void EscapeFromPlayer()
    {
        if (isDead) return;

        isEscaping = true;
        Vector3 directionAwayFromPlayer = (transform.position - player.transform.position).normalized;
        Vector3 escapePosition = transform.position + directionAwayFromPlayer * escapeRange;

        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(escapePosition);
        animator.SetBool("Run", true);
    }

    void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true;
        animator.SetBool("Run", false);
        animator.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }
}
