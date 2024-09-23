using Unity.Netcode;
using UnityEngine;

public class NPCSpawner : NetworkBehaviour
{
    public GameObject npcPrefab; // NPC prefab'ı
    public int npcCount = 5; // Spawn edilecek NPC sayısı
    public Vector3 spawnArea = new Vector3(10f, 0f, 10f); // Spawn alanı boyutu

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Sadece sunucu bu fonksiyonu çalıştırabilir
        {
            SpawnNPCs();
        }
    }

    private void SpawnNPCs()
    {
        Debug.Log("NPC'ler spawn ediliyor...");
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                0.5f, // Y pozisyonunu ayarlayın
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            GameObject npcInstance = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
            npcInstance.GetComponent<NetworkObject>().Spawn(); // NPC'yi ağda spawn et
            Debug.Log($"NPC oluşturuldu: {npcInstance.name} pozisyon: {spawnPosition}");
        }
    }
}
