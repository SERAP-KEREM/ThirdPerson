using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class MafiaController : NetworkBehaviour
{
    public GameObject player; // Oyuncu referansı
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
        player = GameObject.FindGameObjectWithTag("Character");
    }

    void Start()
    {
        initialPosition = transform.position;
        navMeshAgent.speed = runSpeed;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (!IsServer || player == null) return;

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);
        float distanceToInitialPosition = Vector3.Distance(transform.position, initialPosition);

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
            if (distanceToInitialPosition <= 1f)
            {
                navMeshAgent.isStopped = true;
                animator.SetBool("Run", false);
                isReturning = false;
            }
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            if (Input.GetKeyDown(KeyCode.H)) // Hasar tetikleyici
            {
                TakeDamage(20);  // Hasar miktarı
            }
        }
        else if (distanceToPlayer > returnRange)
        {
            isReturning = true;
        }
    }

    [ServerRpc]
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
