using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MafiaController : MonoBehaviour
{
    public GameObject player;  // Oyuncu nesnesi
    public float speed = 5f;   // D�?man?n ko?ma h?z?
    public float attackRange = 10f;  // Ate? etme mesafesi
    public float chaseRange = 20f;   // Ko?maya ba?lama mesafesi
    public float fireRate = 1f;      // Ate? etme s?kl??? (saniyede bir)
    public float damage = 5f;        // Her ate?te verilen hasar
    public float health = 20f;       // D�?man?n can?

    private float nextFireTime = 0f; // Bir sonraki ate? etme zaman?
    private Animator animator;       // Animator referans?
    private bool isDead = false;     // �l�m durumu kontrol�

    Character character;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").gameObject;
        animator = GetComponent<Animator>(); // Animator bile?enini al?yoruz
    }

    void Update()
    {
        if (!isDead)
        {
            // Oyuncu ve d�?man aras?ndaki mesafeyi hesapla
            float distance = Vector3.Distance(gameObject.transform.position, player.transform.position);

            // E?er oyuncu chaseRange (20 birim) i�inde ise d�?man ko?maya ba?las?n
            if (distance < chaseRange && distance > attackRange)
            {
                ChasePlayer(); // Oyuncuya do?ru ko?ma fonksiyonu
            }
            // E?er mesafe attackRange (10 birim) i�inde ise ate? etsin
            else if (distance <= attackRange)
            {
                StopAndShoot(); // Ate? etme fonksiyonu
            }
        }
    }

    // Oyuncuya do?ru ko?ma
    void ChasePlayer()
    {
        animator.SetBool("Run", true);  
        Vector3 direction = (player.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    // Ate? etme fonksiyonu
    void StopAndShoot()
    {
        animator.SetBool("Shoot", true);  // Ko?may? durdur, ate?e haz?r
        // E?er ?u anki zaman, bir sonraki ate? etme zaman?ndan b�y�kse ate? et
        if (Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;

         //   character._health -= 5;
        }
    }

    // Hasar alma fonksiyonu
    public void TakeDamage(float amount)
    {
        health -= amount;
      

        if (health <= 0f && !isDead)
        {
            Die();
        }
    }

    // �lme fonksiyonu
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");  // �l�m animasyonunu tetikle
        Destroy(gameObject, 2f); // 2 saniye sonra yok et
    }
}
