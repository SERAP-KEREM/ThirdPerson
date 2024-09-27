using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Bodyguard : MonoBehaviour
{
    public float health = 100f;               // NPC'nin can?
    public float fleeDistance = 10f;          // Kaçma mesafesi
    public float returnDistance = 15f;        // Geri dönme mesafesi
    public float fleeSpeed = 6f;              // Kaçarken h?z
    public float walkSpeed = 3.5f;            // Yürüyü? h?z?

    public Transform waypointParent;           // Waypoint'lerin parent'?
    private List<Transform> waypoints = new List<Transform>();  // Waypoint listesi
    public Animator animator;                  // NPC animatörü
    private NavMeshAgent navMeshAgent;        // NavMeshAgent bile?eni
    private bool isFleeing = false;           // NPC kaç?yor mu?
    private bool isDead = false;              // NPC ölü mü?
    private Vector3 initialPosition;          // NPC'nin ba?lang?ç pozisyonu
    private int currentWaypointIndex = 0;     // ?u anki waypoint indeksi

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        initialPosition = transform.position;

        // NavMesh kontrolü
        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NPC NavMesh üzerinde de?il!");
        }

        GetWaypointsFromParent();

        // Oyun ba?lad???nda idle animasyonunu ayarla
        animator.SetBool("Run", false); // Idle animasyonu
    }

    // Parent GameObject'in alt?ndaki waypoint'leri al
    private void GetWaypointsFromParent()
    {
        if (waypointParent == null)
        {
            Debug.LogError("Waypoint parent atanmad?.");
            return;
        }

        foreach (Transform child in waypointParent)
        {
            waypoints.Add(child);
        }

        Debug.Log("Toplam Waypoint Say?s?: " + waypoints.Count);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        health -= damageAmount;
        Debug.Log($"NPC can?: {health}");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log("NPC kaç?yor.");
            animator.SetBool("Run", true); // Ko?ma animasyonuna geç
            StartCoroutine(FleeToNearestWaypoint());
        }
    }

    private IEnumerator FleeToNearestWaypoint()
    {
        if (navMeshAgent == null || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogError("NavMeshAgent atanmad? ya da NavMesh üzerinde de?il.");
            yield break;
        }

        navMeshAgent.speed = fleeSpeed;
        isFleeing = true;

        Transform nearestWaypoint = GetNearestWaypoint();

        if (nearestWaypoint != null)
        {
            navMeshAgent.SetDestination(nearestWaypoint.position);

            // Kaç?? süresi
            yield return new WaitForSeconds(15f);
        }

        if (!isDead)  // NPC hala hayatta m??
        {
            ReturnToInitialPosition();
        }
    }

    private Transform GetNearestWaypoint()
    {
        Transform nearestWaypoint = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Transform waypoint in waypoints)
        {
            float distance = Vector3.Distance(transform.position, waypoint.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestWaypoint = waypoint;
            }
        }

        return nearestWaypoint;
    }

    private void ReturnToInitialPosition()
    {
        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.speed = walkSpeed;
            navMeshAgent.SetDestination(initialPosition);
            animator.SetBool("Run", false); // Idle animasyonuna dön

            if (Vector3.Distance(transform.position, initialPosition) > returnDistance)
            {
                StartCoroutine(PatrolWaypoints()); // Waypointler aras?nda dola?
            }
        }
    }

    private IEnumerator PatrolWaypoints()
    {
        while (!isDead)
        {
            if (waypoints.Count == 0) yield break; // E?er waypoint yoksa ç?k

            // ?u anki waypoint'e git
            navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);
            animator.SetBool("Run", false); // Normal h?zda yürüyü? animasyonunu etkinle?tir

            // Waypoint'e ula?ana kadar bekle
            while (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) > 1f)
            {
                yield return null; // Bir sonraki frame'i bekle
            }

            // Hedef waypoint'e ula??ld???nda, s?radaki waypoint'e geç
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count; // Sonraki waypoint'e geç
            yield return new WaitForSeconds(2f); // Waypoint aras?nda biraz bekle
        }
    }

    private void Die()
    {
        isDead = true;
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }
}
