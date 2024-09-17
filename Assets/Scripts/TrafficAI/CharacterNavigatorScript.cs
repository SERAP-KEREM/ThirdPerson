using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterNavigatorScript : MonoBehaviour
{
    [Header("Character Info")]
    public float movingSpeed = 5f;
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
    public float minimumMovementDistance = 2f; // 2 birimden az hareket ederse takıldı sayılacak

    [Header("Raycast Settings")]
    public float raycastDistance = 1.5f; // Raycast'in mesafesi
    public LayerMask obstacleLayerMask; // Engel olarak kabul edilecek katmanlar

    private void Start()
    {
        // İlk pozisyonu sakla
        lastPosition = transform.position;
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
        // Karakter minimum hareket mesafesinden (2 birim) az hareket ettiyse
        if (Vector3.Distance(transform.position, lastPosition) < minimumMovementDistance && !destinationReached)
        {
            timeStuck += Time.deltaTime;

            // 2 saniye boyunca takılı kaldıysa etrafından dolan
            if (timeStuck >= obstacleWaitTime && !isStuck)
            {
                isStuck = true;
                Debug.Log("Engelde takıldı, etrafından dolanılıyor...");

                // Engeli aşmak için etrafından dolan
                AvoidObstacle();

                // Yeniden hareket etmeyi dene
                timeStuck = 0f;
            }

            // Eğer 5 saniyeden fazla aynı pozisyonda kaldıysa bir sonraki waypoint'e geç
            if (timeStuck >= maxStuckTime)
            {
                Debug.Log("Karakter 5 saniyeden fazla aynı pozisyonda kaldı, bir sonraki hedefe geçiliyor...");
                GoToNextWaypoint();
                timeStuck = 0f;
            }
        }
        else
        {
            // Pozisyon değiştiyse zamanı sıfırla
            timeStuck = 0f;
        }

        // Pozisyonu güncelle
        lastPosition = transform.position;
    }

    // Engelin etrafından dolanma fonksiyonu
    private void AvoidObstacle()
    {
        // Engeli aşmak için rastgele bir yana hareket etme stratejisi
        float randomDirection = Random.Range(-1f, 1f);
        transform.Translate(Vector3.right * randomDirection * 2f); // X ekseninde 2 birim yana hareket et
    }

    // Bir sonraki waypoint'e geçişi sağlayan fonksiyon
    private void GoToNextWaypoint()
    {
        // Bir sonraki hedefin belirlenmesi kodu burada olmalı
        LocalDestination(new Vector3(destination.x + 5f, destination.y, destination.z + 5f)); // Örnek bir yeni hedef
    }

    // Karakterin önünde engel olup olmadığını kontrol eden fonksiyon
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

    public void characterHitDamage(float takeDamage)
    {
        presentHealth -= takeDamage;

        if (presentHealth <= 0)
        {
            animator.SetBool("Die", true);
            characterDie();
        }
    }

    private void characterDie()
    {
        movingSpeed = 0f;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        Object.Destroy(gameObject, 4.0f);
    }
}
