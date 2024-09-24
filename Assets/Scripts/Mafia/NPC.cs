using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class NPC : NetworkBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeDistance = 10f;          // Ka�?? mesafesi
    public float returnDistance = 15f;        // Geri d�n�? i�in mesafe
    public float safeDistance = 20f;          // NPC'nin player'dan uzakla?mas? gereken mesafe
    public float speed = 5f;                  // NPC h?z?

    public Animator animator;                 // NPC animat�r�
    public Vector3 initialPosition;           // Ba?lang?� pozisyonu
    private NavMeshAgent navMeshAgent;        // NavMeshAgent bile?eni
    private bool isFleeing = false;           // NPC ka�?yor mu?
    private bool isReturning = false;         // NPC geri d�n�yor mu?
    private bool isDead = false;              // NPC �l� m�?
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

        // Ba?lang?� pozisyonunu kaydet
        initialPosition = transform.position;

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
            playerTarget = character.transform.position;
            Flee(playerTarget);  // NPC ka�maya ba?lar
        }
    }

    // Ka�ma i?lemi
    public void Flee(Vector3 target)
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent atanmad? ya da NavMesh �zerinde de?il.");
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

        // E?er player ile yeterli mesafeye ula?t?ysa geri d�n
        if (Vector3.Distance(transform.position, playerTarget) > safeDistance)
        {
            ReturnToInitialPosition();
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
        navMeshAgent.isStopped = true;  // NPC hareketi durur
        animator.SetTrigger("Die");      // �l�m animasyonu tetiklenir
        Destroy(gameObject, 3f);         // 3 saniye sonra NPC yok olur
    }
}
