using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNavigatorScript : MonoBehaviour
{
    [Header("Character Info")]
    public float movingSpeed;   // Normal yürüme hızı
    public float walkingSpeed = 3f;   // Normal yürüme hızı
    public float runningSpeed = 5f;  // Koşma hızı
    public float turningSpeed = 300f;
    public float stopSpeed = 1f;
    private float characterHealth = 100f;
    public float presentHealth;

    [Header("Destination Var")]
    public Vector3 destination;
    public bool destinationReached;
    public Animator animator;

    [Header("Obstacle Handling")]
    public float obstacleWaitTime = 2f; // Engelde takılma süresi
    public float maxStuckTime = 5f; // 5 saniye boyunca aynı yerde kalma süresi
    private float timeStuck = 0f;
    private bool isStuck = false; // Takılıp kalma durumu

    private Vector3 lastPosition;
    public float minimumMovementDistance = 0.1f; // 0.1 birimden az hareket ederse takıldı sayılacak

    [Header("Raycast Settings")]
    public float raycastDistance = 1.5f; // Raycast'in mesafesi
    public LayerMask obstacleLayerMask; // Engel olarak kabul edilecek katmanlar

    private void Start()
    {
        // İlk pozisyonu sakla
        lastPosition = transform.position;
        presentHealth = characterHealth; // Başlangıçta mevcut sağlık karakterin sağlığına eşit olmalı
    }

    private void Update()
    {
        Walk();
        HandleObstacle();
    }

    public void Walk()
    {
        if (transform.position != destination)
        {
            Vector3 destinationDirection = destination - transform.position;
            destinationDirection.y = 0;
            float destinationDistance = destinationDirection.magnitude;

            if (destinationDistance >= stopSpeed)
            {
                // Dönüş hareketi
                destinationReached = false;
                Quaternion targetRotation = Quaternion.LookRotation(destinationDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);

                // AI Hareketi
                if (!IsObstacleAhead())
                {
                    // Karakterin hareket etmesi
                    transform.Translate(Vector3.forward * movingSpeed * Time.deltaTime);
                }

                // Hareket ettiyse, takılma zamanını sıfırla
                timeStuck = 0f;
                isStuck = false;
            }
            else
            {
                // Hedefe ulaşıldı
                destinationReached = true;
            }
        }
    }

    // Engellerde takılma kontrolü ve çözümü
    private void HandleObstacle()
    {
        if (Vector3.Distance(transform.position, lastPosition) < minimumMovementDistance && !destinationReached)
        {
            timeStuck += Time.deltaTime;

            if (timeStuck >= obstacleWaitTime && !isStuck)
            {
                isStuck = true;
                Debug.Log("Engelde takıldı, etrafından dolanılıyor...");

                // Engeli aşmak için etrafından dolan
                AvoidObstacle();

                timeStuck = 0f;
            }

            if (timeStuck >= maxStuckTime)
            {
                Debug.Log("Karakter 5 saniyeden fazla aynı pozisyonda kaldı, bir sonraki hedefe geçiliyor...");
                GoToNextWaypoint();
                timeStuck = 0f;
            }
        }
        else
        {
            timeStuck = 0f;
        }

        lastPosition = transform.position;
    }

    private void AvoidObstacle()
    {
        float randomDirectionX = Random.Range(-1f, 1f);
        float randomDirectionZ = Random.Range(-1f, 1f);
        Vector3 randomDirection = new Vector3(randomDirectionX, 0, randomDirectionZ).normalized;
        transform.Translate(randomDirection * 2f); // 2 birimlik bir hareket
    }

    private void GoToNextWaypoint()
    {
        LocalDestination(new Vector3(destination.x + 5f, destination.y, destination.z + 5f));
    }

    private bool IsObstacleAhead()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, raycastDistance, obstacleLayerMask))
        {
            Debug.Log("Engel tespit edildi: " + hit.collider.name);
            return true; // Engel var
        }
        return false; // Engel yok
    }

    public void LocalDestination(Vector3 destination)
    {
        this.destination = destination;
        destinationReached = false;
    }

    // Hasar aldığında 15 saniye boyunca koşma ve sonra yürümeye devam etme işlemi
    public void CharacterHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;

        if (presentHealth > 0)
        {
            // Sağlık sıfır değilse, karakter koşmaya başlar
            StartCoroutine(RunForTime(15f));
        }
        else
        {
            // Eğer sağlık sıfırsa karakter ölür
            animator.SetBool("Die", true);
            characterDie();
        }
    }

    // Karakterin 15 saniye boyunca koşmasını sağlayan Coroutine
    private IEnumerator RunForTime(float runDuration)
    {
        movingSpeed = runningSpeed;  // Koşma hızına geçiş
        animator.SetBool("Escape", true);  // Koşma animasyonunu başlat
        Debug.Log("Escape");
        yield return new WaitForSeconds(runDuration);

        movingSpeed = walkingSpeed;  // Tekrar yürüme hızına geçiş
        animator.SetBool("Escape", false);  // Koşma animasyonunu durdur
    }

    private void characterDie()
    {
        movingSpeed = 0f;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        Object.Destroy(gameObject, 4.0f);
    }
}
