using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class Bodyguard : NetworkBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeDistance = 10f;          // Kaç?? mesafesi
    public float returnDistance = 15f;         // Geri dönü? için mesafe
    public float safeDistance = 20f;           // NPC'nin player'dan uzakla?mas? gereken mesafe
    public float speed = 5f;                   // NPC h?z?

    public Animator animator;                   // NPC animatörü
    public Vector3 initialPosition;             // Ba?lang?ç pozisyonu
    private Quaternion initialRotation;         // Ba?lang?ç rotas?
    private NavMeshAgent navMeshAgent;         // NavMeshAgent bile?eni
    private bool isFleeing = false;            // NPC kaç?yor mu?
    private bool isReturning = false;          // NPC geri dönüyor mu?
    private bool isDead = false;               // NPC ölü mü?
    private Transform playerTransform;          // Oyuncu transformu

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;  // Ba?lang?ç pozisyonunu kaydet
        initialRotation = transform.rotation;  // Ba?lang?ç rotas?n? kaydet
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // Oyuncu transformunu al

        // E?er NavMesh üzerinde de?ilse hata mesaj? göster
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh üzerinde de?il!");
        }
    }

    public void TakeDamage(float damageAmount)
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
            Debug.Log("kaç");
            StartCoroutine(FleeCoroutine(playerTransform.position));
        }
    }

    // Kaçma i?lemi
    public void Flee(Vector3 target)
    {
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent atanmad?.");
            return;
        }
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh üzerinde de?il!");
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
            // E?er kaçma mesafesine ula?t?ysa geri dön
            if (Vector3.Distance(transform.position, initialPosition) > returnDistance)
            {
                ReturnToInitialPosition();
            }
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) > safeDistance)
        {
            ReturnToInitialPosition(); // E?er player ile yeterli mesafeye ula?t?ysa geri dön
        }

        if (isReturning)
        {
            // E?er NPC ba?lang?ç pozisyonuna geri döndüyse
            if (Vector3.Distance(transform.position, initialPosition) < 0.5f)
            {
                animator.SetBool("Run", false);  // Kaçma animasyonunu durdur
                navMeshAgent.ResetPath();         // Hareketi durdur
                transform.rotation = initialRotation; // Rotay? geri yükle
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
        navMeshAgent.isStopped = true;  // NPC hareketi durur
        animator.SetTrigger("Die");      // Ölüm animasyonu tetiklenir
        Destroy(gameObject, 3f);         // 3 saniye sonra NPC yok olur
    }

    private IEnumerator FleeCoroutine(Vector3 target)
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent atanmad? ya da NavMesh üzerinde de?il.");
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

        ReturnToInitialPosition();  // Geri dön
    }
}
