using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Bodyguard : MonoBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeSpeed = 6f;              // Kaçarken h?z
    public float fleeDuration = 15f;          // Kaçma süresi
    public Transform fleeTarget;               // Kaç?? hedefi (belirli bir nokta)

    public Animator animator;                  // NPC animatörü
    private NavMeshAgent navMeshAgent;        // NavMeshAgent bile?eni
    private bool isDead = false;              // NPC ölü mü?
    private Vector3 initialPosition;           // Ba?lang?ç pozisyonu
    private Quaternion initialRotation;        // Ba?lang?ç rotasyonu

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;  // Ba?lang?ç rotasyonunu sakla

        // NavMesh kontrolü
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh üzerinde de?il!");
        }

        // Oyun ba?lad???nda idle animasyonunu ayarla
        animator.SetBool("Run", false); // Idle animasyonu
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;  // E?er NPC ölü ise ç?k

        health -= damageAmount;
        Debug.Log($"NPC can?: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log("NPC kaç?yor.");
            StartCoroutine(FleeToTarget());
        }
    }

    private IEnumerator FleeToTarget()
    {
        // Ko?ma animasyonunu ba?lat
        animator.SetBool("Run", true);
        navMeshAgent.speed = fleeSpeed;

        // Kaç?? hedefine git
        if (fleeTarget != null)
        {
            navMeshAgent.SetDestination(fleeTarget.position);
            Debug.Log("NPC hedefe do?ru kaç?yor: " + fleeTarget.position); // Debug mesaj?

            float fleeTimer = 0f;

            while (fleeTimer < fleeDuration)
            {
                fleeTimer += Time.deltaTime;

                // Hedefe ula?may? kontrol et
                // NavMeshAgent'?n hedefe mesafesi kontrolü
                if (Vector3.Distance(transform.position, fleeTarget.position) <= 0.1f)
                {
                    Debug.Log("NPC hedefe ula?t?."); // Debug mesaj?
                    break; // E?er hedefe ula?t?ysa döngüyü k?r
                }

                yield return null; // Bir sonraki frame'e geç
            }
        }
        else
        {
            Debug.LogError("Flee target atanmad?!"); // Hedef atanmad?ysa hata mesaj?
        }

        // Kaç?? tamamland?ktan sonra geri dön
        ReturnToInitialPosition();
    }

    private void ReturnToInitialPosition()
    {
        // Ba?lang?ç konumuna geri dön
        navMeshAgent.SetDestination(initialPosition); // NPC'nin ba?lang?ç konumuna git

        // Geri dönene kadar bekle
        StartCoroutine(WaitForReturn());
    }

    private IEnumerator WaitForReturn()
    {
        // NPC geri dönene kadar bekle
        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null; // NPC geri dönene kadar bekle
        }

        // NPC geri döndü, animasyonu durdur
        transform.position = initialPosition; // Eski pozisyona dön
        transform.rotation = initialRotation; // Eski rotasyona dön
        animator.SetBool("Run", false); // Idle animasyonu
    }

    private void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }
}
