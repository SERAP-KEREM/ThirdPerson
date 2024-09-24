using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class NPC : NetworkBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeDistance = 10f;          // Kaç?? mesafesi
    public float returnDistance = 15f;        // Geri dönü? için mesafe
    public float safeDistance = 20f;          // NPC'nin player'dan uzakla?mas? gereken mesafe
    public float speed = 5f;                  // NPC h?z?

    public Animator animator;                 // NPC animatörü
    public Vector3 initialPosition;           // Ba?lang?ç pozisyonu
    private NavMeshAgent navMeshAgent;        // NavMeshAgent bile?eni
    private bool isFleeing = false;           // NPC kaç?yor mu?
    private bool isReturning = false;         // NPC geri dönüyor mu?
    private bool isDead = false;              // NPC ölü mü?
    private Vector3 playerTarget;             // Player'?n pozisyonu
    private Character character;              // Player'?n Character bile?eni

    private void Start()
    {
        // Player'? bul ve Character bile?enini al
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            character = playerObject.GetComponent<Character>();

            if (character == null)
            {
                Debug.LogError("Character bile?eni bulunamad?!");
            }
        }
        else
        {
            Debug.LogError("Player tag'li obje bulunamad?!");
        }

        // NavMeshAgent bile?enini al
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent bile?eni bulunamad?!");
        }

        // Ba?lang?ç pozisyonunu kaydet
        initialPosition = transform.position;

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
            playerTarget = character.transform.position;
            Flee(playerTarget);  // NPC kaçmaya ba?lar
        }
    }

    // Kaçma i?lemi
    public void Flee(Vector3 target)
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent atanmad? ya da NavMesh üzerinde de?il.");
            return;
        }

        target = character.transform.position;
        Vector3 fleeDirection = (transform.position - target).normalized;
        Vector3 fleeTarget = transform.position + fleeDirection * fleeDistance;

        navMeshAgent.speed = speed;
        navMeshAgent.SetDestination(fleeTarget);
        animator.SetBool("Run", true);
        isFleeing = true;
    }

    private void Update()
    {
        // E?er character veya navMeshAgent atanmad?ysa, i?lemleri durdur
        if (character == null || navMeshAgent == null) return;

        playerTarget = character.transform.position;

        // E?er player ile yeterli mesafeye ula?t?ysa geri dön
        if (Vector3.Distance(transform.position, playerTarget) > safeDistance)
        {
            ReturnToInitialPosition();
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
        navMeshAgent.isStopped = true;  // NPC hareketi durur
        animator.SetTrigger("Die");      // Ölüm animasyonu tetiklenir
        Destroy(gameObject, 3f);         // 3 saniye sonra NPC yok olur
    }
}
