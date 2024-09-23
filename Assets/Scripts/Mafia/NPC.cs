using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class NPC : NetworkBehaviour
{
    public float health = 100f;
    public float fleeDistance = 10f;   // Kaç?? mesafesi
    public float returnDistance = 15f; // Geri dönü? için mesafe
    public float speed = 5f;           // NPC h?z?

    public Animator animator;          // NPC animatörü
    public Vector3 initialPosition;    // Ba?lang?ç pozisyonu (Vector3)
    private NavMeshAgent navMeshAgent;
    private bool isFleeing = false;    // NPC kaç?yor mu?
    private bool isReturning = false;  // NPC geri dönüyor mu?
    private bool isDead = false;       // NPC ölü mü?

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // NPC'nin ba?lang?ç pozisyonunu kaydediyoruz
        initialPosition = transform.position;

        // E?er NavMesh üzerinde de?ilse hata mesaj? gösterelim
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh üzerinde de?il!");
        }
    }

    public void TakeDamage(float damageAmount, Vector3 target)
    {
        if (isDead) return;  // E?er ölü ise daha fazla i?lem yapmay?z
        health -= damageAmount;
        Debug.Log($"NPC can?: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Flee(target);  // NPC kaçmaya ba?lar
        }
    }

    // Kaçma i?lemi
    public void Flee(Vector3 target)
    {
        if (!isFleeing && navMeshAgent.isOnNavMesh)  // NavMesh'te oldu?undan emin ol
        {
            Vector3 fleeDirection = (transform.position - target).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

            navMeshAgent.speed = speed;
            navMeshAgent.SetDestination(fleeTarget);
            animator.SetBool("Run", true);  // Kaçma animasyonu ba?lar
            isFleeing = true;
        }
    }

    private void Update()
    {
        if (isFleeing)
        {
            // E?er kaçma mesafesine ula?m??sa geri dön
            if (Vector3.Distance(transform.position, initialPosition) > returnDistance)
            {
                ReturnToInitialPosition();
            }
        }

        if (isReturning)
        {
            // E?er NPC ba?lang?ç pozisyonuna geri döndüyse
            if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
            {
                animator.SetBool("Run", false);  // Kaçma animasyonunu durdur
                animator.SetBool("Idle", true);  // Idle animasyonu ba?lar
                navMeshAgent.ResetPath();        // Hareketi durdur
                isReturning = false;
                isFleeing = false;
            }
        }
    }

    // Ba?lang?ç pozisyonuna geri dönme i?lemi
    private void ReturnToInitialPosition()
    {
        if (navMeshAgent.isOnNavMesh)  // NavMesh üzerinde oldu?undan emin olun
        {
            navMeshAgent.SetDestination(initialPosition);
            isReturning = true;
            isFleeing = false;
        }
    }

    // Ölüm i?lemi
    private void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true; // NPC hareketi durur
        animator.SetTrigger("Die");    // Ölüm animasyonu tetiklenir
        Destroy(gameObject, 3f);       // 3 saniye sonra NPC yok olur
    }
}

