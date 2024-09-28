using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Bodyguard : MonoBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeSpeed = 6f;              // Ka�arken h?z
    public float fleeDuration = 15f;          // Ka�ma s�resi
    public Transform fleeTarget;               // Ka�?? hedefi (belirli bir nokta)

    public Animator animator;                  // NPC animat�r�
    private NavMeshAgent navMeshAgent;        // NavMeshAgent bile?eni
    private bool isDead = false;              // NPC �l� m�?
    private Vector3 initialPosition;           // Ba?lang?� pozisyonu
    private Quaternion initialRotation;        // Ba?lang?� rotasyonu

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;  // Ba?lang?� rotasyonunu sakla

        // NavMesh kontrol�
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh �zerinde de?il!");
        }

        // Oyun ba?lad???nda idle animasyonunu ayarla
        animator.SetBool("Run", false); // Idle animasyonu
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;  // E?er NPC �l� ise �?k

        health -= damageAmount;
        Debug.Log($"NPC can?: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log("NPC ka�?yor.");
            StartCoroutine(FleeToTarget());
        }
    }

    private IEnumerator FleeToTarget()
    {
        // Ko?ma animasyonunu ba?lat
        animator.SetBool("Run", true);
        navMeshAgent.speed = fleeSpeed;

        // Ka�?? hedefine git
        if (fleeTarget != null)
        {
            navMeshAgent.SetDestination(fleeTarget.position);
            Debug.Log("NPC hedefe do?ru ka�?yor: " + fleeTarget.position); // Debug mesaj?

            float fleeTimer = 0f;

            while (fleeTimer < fleeDuration)
            {
                fleeTimer += Time.deltaTime;

                // Hedefe ula?may? kontrol et
                // NavMeshAgent'?n hedefe mesafesi kontrol�
                if (Vector3.Distance(transform.position, fleeTarget.position) <= 0.1f)
                {
                    Debug.Log("NPC hedefe ula?t?."); // Debug mesaj?
                    break; // E?er hedefe ula?t?ysa d�ng�y� k?r
                }

                yield return null; // Bir sonraki frame'e ge�
            }
        }
        else
        {
            Debug.LogError("Flee target atanmad?!"); // Hedef atanmad?ysa hata mesaj?
        }

        // Ka�?? tamamland?ktan sonra geri d�n
        ReturnToInitialPosition();
    }

    private void ReturnToInitialPosition()
    {
        // Ba?lang?� konumuna geri d�n
        navMeshAgent.SetDestination(initialPosition); // NPC'nin ba?lang?� konumuna git

        // Geri d�nene kadar bekle
        StartCoroutine(WaitForReturn());
    }

    private IEnumerator WaitForReturn()
    {
        // NPC geri d�nene kadar bekle
        while (navMeshAgent.pathPending || navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null; // NPC geri d�nene kadar bekle
        }

        // NPC geri d�nd�, animasyonu durdur
        transform.position = initialPosition; // Eski pozisyona d�n
        transform.rotation = initialRotation; // Eski rotasyona d�n
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
