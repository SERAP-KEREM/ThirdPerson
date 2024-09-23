using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class NPC : NetworkBehaviour
{
    public float health = 100f;
    public float fleeDistance = 10f;   // Ka�?? mesafesi
    public float returnDistance = 15f; // Geri d�n�? i�in mesafe
    public float speed = 5f;           // NPC h?z?

    public Animator animator;          // NPC animat�r�
    public Vector3 initialPosition;    // Ba?lang?� pozisyonu (Vector3)
    private NavMeshAgent navMeshAgent;
    private bool isFleeing = false;    // NPC ka�?yor mu?
    private bool isReturning = false;  // NPC geri d�n�yor mu?
    private bool isDead = false;       // NPC �l� m�?

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        // NPC'nin ba?lang?� pozisyonunu kaydediyoruz
        initialPosition = transform.position;

        // E?er NavMesh �zerinde de?ilse hata mesaj? g�sterelim
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh �zerinde de?il!");
        }
    }

    public void TakeDamage(float damageAmount, Vector3 target)
    {
        if (isDead) return;  // E?er �l� ise daha fazla i?lem yapmay?z
        health -= damageAmount;
        Debug.Log($"NPC can?: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Flee(target);  // NPC ka�maya ba?lar
        }
    }

    // Ka�ma i?lemi
    public void Flee(Vector3 target)
    {
        if (!isFleeing && navMeshAgent.isOnNavMesh)  // NavMesh'te oldu?undan emin ol
        {
            Vector3 fleeDirection = (transform.position - target).normalized;
            Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

            navMeshAgent.speed = speed;
            navMeshAgent.SetDestination(fleeTarget);
            animator.SetBool("Run", true);  // Ka�ma animasyonu ba?lar
            isFleeing = true;
        }
    }

    private void Update()
    {
        if (isFleeing)
        {
            // E?er ka�ma mesafesine ula?m??sa geri d�n
            if (Vector3.Distance(transform.position, initialPosition) > returnDistance)
            {
                ReturnToInitialPosition();
            }
        }

        if (isReturning)
        {
            // E?er NPC ba?lang?� pozisyonuna geri d�nd�yse
            if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
            {
                animator.SetBool("Run", false);  // Ka�ma animasyonunu durdur
                animator.SetBool("Idle", true);  // Idle animasyonu ba?lar
                navMeshAgent.ResetPath();        // Hareketi durdur
                isReturning = false;
                isFleeing = false;
            }
        }
    }

    // Ba?lang?� pozisyonuna geri d�nme i?lemi
    private void ReturnToInitialPosition()
    {
        if (navMeshAgent.isOnNavMesh)  // NavMesh �zerinde oldu?undan emin olun
        {
            navMeshAgent.SetDestination(initialPosition);
            isReturning = true;
            isFleeing = false;
        }
    }

    // �l�m i?lemi
    private void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true; // NPC hareketi durur
        animator.SetTrigger("Die");    // �l�m animasyonu tetiklenir
        Destroy(gameObject, 3f);       // 3 saniye sonra NPC yok olur
    }
}

