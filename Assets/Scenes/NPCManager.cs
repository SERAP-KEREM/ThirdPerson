using Unity.Netcode;
using UnityEngine;

public class NPCManager : NetworkBehaviour
{
    [SerializeField] private GameObject npcPrefab; // Inspector �zerinden atanacak prefab
    public Vector3 spawnPosition; // NPC'nin spawn edilece?i konum

    void Start()
    {
        SpawnNPC(); // Oyun ba?larken NPC spawn et
    }

    // NPC'yi spawn etme fonksiyonu
    public void SpawnNPC()
    {
        if (IsServer) // Sadece sunucu bu fonksiyonu �a??rabilir
        {
            GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
            npc.GetComponent<NetworkObject>().Spawn(); // NPC'yi a?a dahil et
        }
    }

    void Update()
    {
        // �rne?in, "N" tu?una bas?ld???nda NPC'yi spawn etme
        if (IsOwner && Input.GetKeyDown(KeyCode.N))
        {
            SpawnNPC();
        }
    }
}
