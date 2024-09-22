using UnityEngine;
using Unity.Netcode;

public class NPCSpawner : NetworkBehaviour
{
    public GameObject npcPrefab; // NPC prefab'?
    public int npcCount = 5; // Spawn edilecek NPC say?s?

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Tüm oyuncular?n yüklendi?inden emin olmak için k?sa bir gecikme ekleyelim
            Invoke(nameof(SpawnNPCs), 1f);
        }
    }

    private void SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            GameObject npc = Instantiate(npcPrefab, GetRandomPosition(), Quaternion.identity);
            npc.GetComponent<NetworkObject>().Spawn(); // NPC'yi a?da tan?mla
        }
    }

    private Vector3 GetRandomPosition()
    {
        // Burada NPC'nin rastgele bir pozisyona yerle?ece?i bir fonksiyon yazabilirsin
        return new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
    }
}
