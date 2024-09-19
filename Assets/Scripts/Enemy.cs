using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Health and Damage")]
    private float enemyHealth = 120f;
    private float presentHealth;
    public float giveDamage = 5f;
    public float enemySpeed;

    [Header("Enemy Things")]
    public NavMeshAgent enemyAgent;
    public Transform LookPoint;
    public GameObject ShootingRaycastArea;
    public Transform playerBody;
    public LayerMask PlayerLayer;
    public Transform Spawn;
    public Transform EnemyCharacter;

    [Header("Enemy Shooting Var")]
    public float timebtwShoot;
    bool previouslyShoot;

    [Header("Enemy Animation and Spark Effect")]
    public Animator animator;

    [Header("Enemy States")]
    public float visionRadius;
    public float shootingRadius;
    public bool playerInvisionRadius;
    public bool playerInshootingRadius;
    public bool isPlayer = false;

    private void Awake()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        presentHealth = enemyHealth;
    }

    private void Update()
    {
        playerInvisionRadius = Physics.CheckSphere(transform.position, visionRadius, PlayerLayer);
        playerInshootingRadius = Physics.CheckSphere(transform.position, shootingRadius, PlayerLayer);

        if (playerInvisionRadius && !playerInshootingRadius)
        {
            Pursueplayer();
        }
        else if (playerInvisionRadius && playerInshootingRadius)
        {
            ShootPlayer();
        }
        else
        {
            animator.SetBool("Running", false);
            animator.SetBool("Shooting", false);
        }
    }

    private void Pursueplayer()
    {
        if (enemyAgent.SetDestination(playerBody.position))
        {
            animator.SetBool("Running", true);
            animator.SetBool("Shooting", false);
        }
        else
        {
            animator.SetBool("Running", false);
            animator.SetBool("Shooting", false);
        }
    }

    private void ShootPlayer()
    {
        enemyAgent.SetDestination(transform.position);

        // Düşmanı oyuncuya bakacak şekilde döndür
        Vector3 direction = (playerBody.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        // Silahın yönünü oyuncuya doğru döndür
        ShootingRaycastArea.transform.LookAt(playerBody.position);

        if (!previouslyShoot)
        {
            RaycastHit hit;

            // Silahın yönünü tekrar kontrol et ve ayarla
            Vector3 shootDirection = (playerBody.position - ShootingRaycastArea.transform.position).normalized;

            // Debug.DrawRay ile raycast'i görselleştir
            Debug.DrawRay(ShootingRaycastArea.transform.position, shootDirection * shootingRadius, Color.red, 1f);

            if (Physics.Raycast(ShootingRaycastArea.transform.position, shootDirection, out hit, shootingRadius))
            {
                Debug.Log("Shooting " + hit.transform.name);

                Character player = hit.transform.GetComponent<Character>();

                if (player != null)
                {
                   // player.PlayerHitDamage(giveDamage);
                }
            }

            animator.SetBool("Running", false);
            animator.SetBool("Shooting", true);
        }
        previouslyShoot = true;
        Invoke(nameof(ActiveShooting), timebtwShoot);
    }

    private void ActiveShooting()
    {
        previouslyShoot = false;
    }

    //playerHitDamage
    public void EnemyHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;

        if (presentHealth <= 0)
        {
            StartCoroutine(Respawn());
        }
    }

    IEnumerator Respawn()
    {
        enemyAgent.SetDestination(transform.position);
        enemySpeed = 0f;
        shootingRadius = 0f;
        visionRadius = 0f;
        playerInvisionRadius = false;
        playerInshootingRadius = false;
        animator.SetBool("Die", true);
        animator.SetBool("Running", false);
        animator.SetBool("Shooting", false);

        //animations
        Debug.Log("Dead");
        yield return new WaitForSeconds(5f);

        Debug.Log("Spawn");

        presentHealth = 120f;
        enemySpeed = 1f;
        shootingRadius = 10f;
        visionRadius = 100f;
        playerInvisionRadius = true;
        playerInshootingRadius = false;

        //animations

        animator.SetBool("Die", false);
        animator.SetBool("Running", true);

        //spawn point
        EnemyCharacter.transform.position = Spawn.transform.position;
        Pursueplayer();
    }
}
