using UnityEngine;
using Unity.Netcode;

public class GameSetup : MonoBehaviour
{
    public GameObject mafiaPrefab; // Mafia prefab referans?

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnMafiaCharacter();
        }
    }

    void SpawnMafiaCharacter()
    {
        // Prefab'? instantiate et
        GameObject mafia = Instantiate(mafiaPrefab, GetSpawnPosition(), Quaternion.identity);

        // NetworkObject bile?enini al ve spawn et
        NetworkObject networkObject = mafia.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("NetworkObject bile?eni bulunamad?!");
        }
    }

    Vector3 GetSpawnPosition()
    {
        // Spawn pozisyonunu belirleyin (örne?in, rastgele veya belirli bir noktada)
        return new Vector3(0, 0, 0);
    }
}
