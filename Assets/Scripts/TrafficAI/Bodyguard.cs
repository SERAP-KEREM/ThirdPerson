using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class Bodyguard : NetworkBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeDistance = 10f;          // Ka�?? mesafesi
    public float returnDistance = 15f;         // Geri d�n�? i�in mesafe
    public float safeDistance = 20f;           // NPC'nin player'dan uzakla?mas? gereken mesafe
    public float speed = 5f;                   // NPC h?z?

    public Animator animator;                   // NPC animat�r�
    public Vector3 initialPosition;             // Ba?lang?� pozisyonu
    private Quaternion initialRotation;         // Ba?lang?� rotas?
    private NavMeshAgent navMeshAgent;         // NavMeshAgent bile?eni
    private bool isFleeing = false;            // NPC ka�?yor mu?
    private bool isReturning = false;          // NPC geri d�n�yor mu?
    private bool isDead = false;               // NPC �l� m�?
    private Transform playerTransform;          // Oyuncu transformu

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;  // Ba?lang?� pozisyonunu kaydet
        initialRotation = transform.rotation;  // Ba?lang?� rotas?n? kaydet
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // Oyuncu transformunu al

        // E?er NavMesh �zerinde de?ilse hata mesaj? g�ster
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh �zerinde de?il!");
        }
    }

    public void TakeDamage(float damageAmount)
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
            Debug.Log("ka�");
            StartCoroutine(FleeCoroutine(playerTransform.position));
        }
    }

    // Ka�ma i?lemi
    public void Flee(Vector3 target)
    {
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent atanmad?.");
            return;
        }
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh �zerinde de?il!");
            return;
        }

        Vector3 fleeDirection = (transform.position - target).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

        navMeshAgent.speed = speed;
        navMeshAgent.SetDestination(fleeTarget);
        animator.SetBool("Run", true);
        isFleeing = true;
    }

    private void Update()
    {
        if (isFleeing)
        {
            // E?er ka�ma mesafesine ula?t?ysa geri d�n
            if (Vector3.Distance(transform.position, initialPosition) > returnDistance)
            {
                ReturnToInitialPosition();
            }
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) > safeDistance)
        {
            ReturnToInitialPosition(); // E?er player ile yeterli mesafeye ula?t?ysa geri d�n
        }

        if (isReturning)
        {
            // E?er NPC ba?lang?� pozisyonuna geri d�nd�yse
            if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
            {
                animator.SetBool("Run", false);  // Ka�ma animasyonunu durdur
                navMeshAgent.ResetPath();         // Hareketi durdur
                transform.rotation = initialRotation; // Rotay? geri y�kle
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
        navMeshAgent.isStopped = true;  // NPC hareketi durur
        animator.SetTrigger("Die");      // �l�m animasyonu tetiklenir
        Destroy(gameObject, 3f);         // 3 saniye sonra NPC yok olur
    }

    private IEnumerator FleeCoroutine(Vector3 target)
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent atanmad? ya da NavMesh �zerinde de?il.");
            yield break;  // Coroutine'i durdur
        }

        Vector3 fleeDirection = (transform.position - target).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

        navMeshAgent.speed = speed;
        navMeshAgent.SetDestination(fleeTarget);
        animator.SetBool("Run", true);
        isFleeing = true;

        // 10 saniye bekle
        yield return new WaitForSeconds(10f);

        ReturnToInitialPosition();  // Geri d�n
    }
}
