using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public float fireRate = 0.5f; // Ate? etme h?z?
    private float nextFireTime = 0f; // Bir sonraki ate? etme zaman?
    public float weaponDamage = 25f; // Verilen hasar miktar?
    public string targetTag = "NPC"; // Ate? edilecek hedefin tag'i

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime) // Sol t?k ile ate?
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            Debug.Log("Ray bir objeye vurdu: " + hit.collider.name); // Raycast'in vurdu?u objeyi yazd?r

            if (hit.collider.CompareTag(targetTag)) // Tag kontrolü
            {
                Debug.Log("Vurulan objenin tag'i: " + targetTag);

                // NPC scriptini al?p hasar vermek
                NPC npc = hit.collider.GetComponent<NPC>();
                if (npc != null)
                {
                    Debug.Log("NPC bulundu, hasar veriliyor...");
                 //   npc.TakeDamageServerRpc(weaponDamage); // Hasar verme i?lemi
                }
                else
                {
                    Debug.LogWarning("Vurulan objede NPC scripti yok!");
                }
            }
            else
            {
                Debug.LogWarning("Vurulan objenin tag'i yanl??: " + hit.collider.tag);
            }
        }
        else
        {
            Debug.LogWarning("Hiçbir ?eye vurulmad?!");
        }
    }
}
