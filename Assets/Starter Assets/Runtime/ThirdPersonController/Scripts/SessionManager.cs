using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using LitJson;
using System;

public class SessionManager : NetworkBehaviour
{
    private static SessionManager _singleton = null;

    public static SessionManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<SessionManager>();
            }
            return _singleton;
        }
    }

    private Dictionary<ulong, Character> _characters = new Dictionary<ulong, Character>();
    public void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.StartServer();
    }



    private void OnClientConnected(ulong clientId)
    {
        ulong[] target = new ulong[1];
        target[0] = clientId;
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = target;
        OnClientConnectedClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void OnClientConnectedClientRpc(ClientRpcParams rpcParams = default)
    {
        //ToDo: Pass the account id
        long accountID = 0;
        SpawnCharacterServerRPC(accountID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCharacterServerRPC(long accountID, ServerRpcParams serverRpcParams = default)
    {
        Character prefab = PrefabManager.singleton.GetCharacterPrefab("Bot");
        if (prefab != null)
        {
            Vector3 position = new Vector3(UnityEngine.Random.Range(-5, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));
            Character character = Instantiate(prefab, position, Quaternion.identity);
            character.GetComponent<NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

            _characters.Add(serverRpcParams.Receive.SenderClientId, character);

            Dictionary<string, int> items = new Dictionary<string, int> { { "ScarL", 30 }, { "7.62x39mm", 1000 } };
            List<string> itemsId = new List<string>();
            List<string> equippedIds = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                itemsId.Add(System.Guid.NewGuid().ToString());
            }
            string itemsJson = JsonMapper.ToJson(items);
            string itemsIdJson = JsonMapper.ToJson(itemsId);
            string equippedJson = JsonMapper.ToJson(equippedIds);

            Item[] allItems = FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            List<Item.Data> itemsOnGround = new List<Item.Data>();
            if (allItems != null)
            {
                for (int i = 0; i < allItems.Length; i++)
                {
                    if (string.IsNullOrEmpty(allItems[i].networkID))
                    {
                        allItems[i].networkID = System.Guid.NewGuid().ToString();
                    }
                    if (allItems[i].transform.parent == null)
                    {
                        itemsOnGround.Add(allItems[i].GetData());
                    }
                }
            }
            string itemsOnGroundJson = JsonMapper.ToJson(itemsOnGround);


            character.InitializeServer(items,itemsId, equippedIds, serverRpcParams.Receive.SenderClientId);   
            character.InitalizeClientRpc(itemsJson,itemsIdJson, equippedJson, itemsOnGroundJson, serverRpcParams.Receive.SenderClientId); 

            foreach (var client in _characters)
            {
                if (client.Value != null && client.Value !=character)
                {
                    Character.Data data = client.Value.GetData();
                    string json = JsonMapper.ToJson(data);
                    ulong[] target = new ulong[1];
                    target[0] = serverRpcParams.Receive.SenderClientId;
                    ClientRpcParams clientRpcParams = default;
                    clientRpcParams.Send.TargetClientIds = target;

                    client.Value.InitalizeClientRpc(json, client.Key, clientRpcParams);
                }
            }


        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    
}
