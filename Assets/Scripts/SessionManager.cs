using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using LitJson;
using DevelopersHub.RealtimeNetworking.Client;
using Unity.Netcode.Transports.UTP;

public class SessionManager : NetworkBehaviour
{

    private float _destroyServerAfterSecondsIfNoClientConnected = 300;
    private float _destroyServerAfterSecondsWithoutAnyClient = 120;
    private float _timer = 0;
    private int _connectedClients = 0;
    private bool _atLeastOneClientConnected = false;
    private bool _closingServer = false;

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

    private static Role _role = Role.Client; public static Role role { get { return _role; } set { _role = value; } }
    private static ushort _port = 0; public static ushort port { get { return _port; } set { _port = value; } }
    private static long _accountID = 0; public static long accountID { get { return _port; } set { _accountID = value; } }

    public enum Role
    {
        Server = 1, Client = 2
    }

    private void Start()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = Client.instance.settings.ip;
        if (role == Role.Server)
        {
            _port = (ushort)DevelopersHub.RealtimeNetworking.Client.Tools.FindFreeTcpPort();
            transport.ConnectionData.Port = _port;
            StartServer();
        }
        else
        {
            transport.ConnectionData.Port = _port;
            StartClient();
        }
    }

    private void Update()
    {
        if (role == Role.Server)
        {
            if (_closingServer)
            {
                return;
            }
            if (_atLeastOneClientConnected)
            {
                if (_connectedClients > 0)
                {
                    _timer = 0;
                }
                else
                {
                    _timer += Time.deltaTime;
                    if (_timer >= _destroyServerAfterSecondsWithoutAnyClient)
                    {
                        CloseServer();
                    }
                }
            }
            else
            {
                _destroyServerAfterSecondsIfNoClientConnected -= Time.deltaTime;
                if (_destroyServerAfterSecondsIfNoClientConnected <= 0)
                {
                    CloseServer();
                }
            }
        }
    }

    public void StartServer()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.StartServer();
        Item[] allItems = FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allItems != null)
        {
            for (int i = 0; i < allItems.Length; i++)
            {
                allItems[i].ServerInitialize();
            }
        }
        StartCoroutine(InformClients());
    }

    private void OnClientDisconnect(ulong obj)
    {
        _connectedClients--;
    }

    private IEnumerator InformClients()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(2f);
        RealtimeNetworking.NetcodeServerIsReady(_port);
    }

    private void CloseServer()
    {
        if (_role == Role.Server)
        {
            if (_closingServer)
            {
                return;
            }
            _closingServer = true;
            RealtimeNetworking.NetcodeCloseServer();
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        _connectedClients++;
        _atLeastOneClientConnected = true;
        ulong[] target = new ulong[1];
        target[0] = clientId;
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = target;
        OnClientConnectedClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void OnClientConnectedClientRpc(ClientRpcParams rpcParams = default)
    {
        SpawnCharacterServerRpc(_accountID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCharacterServerRpc(long accountID, ServerRpcParams serverRpcParams = default)
    {
        DevelopersHub.RealtimeNetworking.Client.Data.RuntimeGame runtimeGame = RealtimeNetworking.NetcodeGetGameData();
        DevelopersHub.RealtimeNetworking.Client.Data.RuntimePlayer player = null;
        for (int i = 0; i < runtimeGame.players.Count; i++)
        {
            if (runtimeGame.players[i].id == accountID)
            {
                player = runtimeGame.players[i];
                break;
            }
        }
        if (player != null && player.characters.Count > 0)
        {
            Character prefab = PrefabManager.singleton.GetCharacterPrefab(player.characters[0].tag);
            if (prefab != null)
            {
                Vector3 position = new Vector3(UnityEngine.Random.Range(-5f, 5f), 0f, UnityEngine.Random.Range(-5f, 5f));

                Character character = Instantiate(prefab, position, Quaternion.identity);
                character.GetComponent<Unity.Netcode.NetworkObject>().SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);

                _characters.Add(serverRpcParams.Receive.SenderClientId, character);

                List<Character.BaseData> items = new List<Character.BaseData>();
                for (int i = 0; i < player.characters[0].equipments.Count; i++)
                {
                    Item equipmentPrefab = PrefabManager.singleton.GetItemPrefab(player.characters[0].equipments[i].tag);
                    if (equipmentPrefab != null)
                    {
                        Character.BaseData data = new Character.BaseData();
                        data.id = player.characters[0].equipments[i].tag;
                        if (equipmentPrefab.GetType() == typeof(Weapon))
                        {
                            data.count = ((Weapon)equipmentPrefab).clipSize;
                        }
                        else if (equipmentPrefab.GetType() == typeof(Ammo))
                        {
                            data.count = 100;
                        }
                        else
                        {
                            data.count = 1;
                        }
                        items.Add(data);
                    }
                }

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


                character.InitializeServer(items, itemsId, equippedIds, serverRpcParams.Receive.SenderClientId);
                character.InitializeClientRpc(itemsJson, itemsIdJson, equippedJson, itemsOnGroundJson, serverRpcParams.Receive.SenderClientId);

                foreach (var client in _characters)
                {
                    if (client.Value != null && client.Value != character)
                    {
                        Character.Data data = client.Value.GetData();
                        string json = JsonMapper.ToJson(data);

                        ulong[] target = new ulong[1];
                        target[0] = serverRpcParams.Receive.SenderClientId;
                        ClientRpcParams clientRpcParams = default;
                        clientRpcParams.Send.TargetClientIds = target;

                        client.Value.InitializeClientRpc(json, client.Key, clientRpcParams);
                    }
                }

            }
        }
        else
        {
            // Problem
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    [System.Serializable]
    public struct TradeItemData
    {
        public Item.Data item;
        public bool merge;
        public string mergeID;
    }

    public void TradeItemsBetweenCharacters(Character character1, Character character2, Dictionary<Item, int> character1To2Items, Dictionary<Item, int> character2To1Items)
    {
        if (character1 == null || character2 == null || character1 == character2)
        {
            return;
        }
        Dictionary<string, int> serializable1To2 = new Dictionary<string, int>();
        Dictionary<string, int> serializable2To1 = new Dictionary<string, int>();
        if (character1To2Items != null)
        {
            foreach (var item in character1To2Items)
            {
                if (item.Value <= 0 && item.Key.GetType() == typeof(Ammo))
                {
                    continue;
                }
                if (item.Key != null && character1.inventory.Contains(item.Key))
                {
                    serializable1To2.Add(item.Key.networkID, item.Value);
                }
            }
        }
        if (character2To1Items != null)
        {
            foreach (var item in character2To1Items)
            {
                if (item.Value <= 0 && item.Key.GetType() == typeof(Ammo))
                {
                    continue;
                }
                if (item.Key != null && character2.inventory.Contains(item.Key))
                {
                    serializable2To1.Add(item.Key.networkID, item.Value);
                }
            }
        }
        if (serializable1To2.Count > 0 || serializable2To1.Count > 0)
        {
            string json1 = JsonMapper.ToJson(serializable1To2);
            string json2 = JsonMapper.ToJson(serializable2To1);
            TradeItemsBetweenCharactersServerRpc(character1.clientID, character2.clientID, json1, json2);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TradeItemsBetweenCharactersServerRpc(ulong character1ID, ulong character2ID, string character1To2Json, string charactre2To1Json)
    {
        Character character1 = null;
        Character character2 = null;
        if (_characters.ContainsKey(character1ID))
        {
            character1 = _characters[character1ID];
        }
        if (_characters.ContainsKey(character2ID))
        {
            character2 = _characters[character2ID];
        }
        if (character1 == null || character2 == null || character1 == character2)
        {
            return;
        }

        Dictionary<string, int> serializable1To2 = JsonMapper.ToObject<Dictionary<string, int>>(character1To2Json);
        Dictionary<string, int> serializable2To1 = JsonMapper.ToObject<Dictionary<string, int>>(charactre2To1Json);
        /*
        Dictionary<string, int> items1To2 = new Dictionary<string, int>();
        Dictionary<string, (string, int)> splitItems1 = new Dictionary<string, (string, int)>();
        Dictionary<string, int> items2To1 = new Dictionary<string, int>();
        Dictionary<string, (string, int)> splitItems2 = new Dictionary<string, (string, int)>();
        */
        List<TradeItemData> items1To2 = new List<TradeItemData>();
        List<Item.Data> splitItems1 = new List<Item.Data>();
        List<TradeItemData> items2To1 = new List<TradeItemData>();
        List<Item.Data> splitItems2 = new List<Item.Data>();

        foreach (var item in serializable1To2)
        {
            for (int i = 0; i < character1.inventory.Count; i++)
            {
                if (item.Key == character1.inventory[i].networkID)
                {
                    int count = item.Value;
                    int remained = 0;
                    int c = 0;
                    if (character1.inventory[i].GetType() == typeof(Weapon))
                    {
                        count = ((Weapon)character1.inventory[i]).ammo;
                    }
                    else
                    {
                        c = character1.inventory[i].GetAmount();
                        if (count <= 0)
                        {
                            break;
                        }
                        else if (c < count)
                        {
                            count = c;
                        }
                        else if (c > count)
                        {
                            remained = c - count;
                            c = count;
                            character1.inventory[i].SetAmount(c);
                        }
                    }
                    if (remained > 0)
                    {
                        Item prefab = PrefabManager.singleton.GetItemPrefab(character1.inventory[i].id);
                        if (prefab != null)
                        {
                            Item splitItem = Instantiate(prefab, transform);
                            splitItem.networkID = System.Guid.NewGuid().ToString();
                            splitItem.SetAmount(remained);
                            character1.AddItemToInventoryLocally(splitItem);
                            splitItems1.Add(splitItem.GetData());
                        }
                        else
                        {
                            break;
                        }
                    }

                    Item merge = null;
                    for (int j = 0; j < character2.inventory.Count; j++)
                    {
                        if (character2.inventory[j].id == character1.inventory[i].id)
                        {
                            merge = character2.inventory[j];
                            break;
                        }
                    }

                    character2.AddItemToInventoryLocally(character1.inventory[i], merge);

                    TradeItemData data = new TradeItemData();
                    data.item = character1.inventory[i].GetData();
                    data.item.value = count;
                    if (merge == null)
                    {
                        data.merge = false;
                    }
                    else
                    {
                        data.merge = true;
                        data.mergeID = merge.networkID;
                    }
                    items1To2.Add(data);

                    character1.RemoveItemFromInventoryLocally(character1.inventory[i]);
                    break;
                }
            }
        }

        foreach (var item in serializable2To1)
        {
            for (int i = 0; i < character2.inventory.Count; i++)
            {
                if (item.Key == character2.inventory[i].networkID)
                {
                    int count = item.Value;
                    int remained = 0;
                    int c = 0;
                    if (character2.inventory[i].GetType() == typeof(Weapon))
                    {
                        count = ((Weapon)character2.inventory[i]).ammo;
                    }
                    else
                    {
                        c = character2.inventory[i].GetAmount();
                        if (count <= 0)
                        {
                            break;
                        }
                        else if (c < count)
                        {
                            count = c;
                        }
                        else if (c > count)
                        {
                            remained = c - count;
                            c = count;
                            character2.inventory[i].SetAmount(c);
                        }
                    }
                    if (remained > 0)
                    {
                        Item prefab = PrefabManager.singleton.GetItemPrefab(character2.inventory[i].id);
                        if (prefab != null)
                        {
                            Item splitItem = Instantiate(prefab, transform);
                            splitItem.networkID = System.Guid.NewGuid().ToString();
                            splitItem.SetAmount(remained);
                            character2.AddItemToInventoryLocally(splitItem);
                            splitItems2.Add(splitItem.GetData());
                        }
                        else
                        {
                            break;
                        }
                    }

                    Item merge = null;
                    for (int j = 0; j < character1.inventory.Count; j++)
                    {
                        if (character1.inventory[j].id == character2.inventory[i].id)
                        {
                            merge = character1.inventory[j];
                            break;
                        }
                    }

                    character1.AddItemToInventoryLocally(character2.inventory[i], merge);

                    TradeItemData data = new TradeItemData();
                    data.item = character2.inventory[i].GetData();
                    data.item.value = count;
                    if (merge == null)
                    {
                        data.merge = false;
                    }
                    else
                    {
                        data.merge = true;
                        data.mergeID = merge.networkID;
                    }
                    items2To1.Add(data);

                    character2.RemoveItemFromInventoryLocally(character2.inventory[i]);
                    break;
                }
            }
        }

        if (items2To1.Count > 0 || items1To2.Count > 0)
        {
            string json1To2 = JsonMapper.ToJson(items1To2);
            string json1Split = JsonMapper.ToJson(splitItems1);
            string json2To1 = JsonMapper.ToJson(items2To1);
            string json2Split = JsonMapper.ToJson(splitItems2);
            TradeItemsBetweenCharactersClientRpc(character1ID, character2ID, json1To2, json1Split, json2To1, json2Split);
        }
    }

    [ClientRpc]
    private void TradeItemsBetweenCharactersClientRpc(ulong character1ID, ulong character2ID, string json1To2, string json1Split, string json2To1, string json2Split)
    {
        Character character1 = null;
        Character character2 = null;
        Character[] allCharacters = FindObjectsByType<Character>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allCharacters != null)
        {
            for (int i = 0; i < allCharacters.Length; i++)
            {
                if (allCharacters[i].clientID == character1ID)
                {
                    character1 = allCharacters[i];
                }
                else if (allCharacters[i].clientID == character2ID)
                {
                    character2 = allCharacters[i];
                }
                if (character1 != null && character2 != null)
                {
                    break;
                }
            }
        }
        if (character1 == null || character2 == null || character1 == character2)
        {
            return;
        }

        List<TradeItemData> items1To2 = JsonMapper.ToObject<List<TradeItemData>>(json1To2);
        List<Item.Data> splitItems1 = JsonMapper.ToObject<List<Item.Data>>(json1Split);
        List<TradeItemData> items2To1 = JsonMapper.ToObject<List<TradeItemData>>(json2To1);
        List<Item.Data> splitItems2 = JsonMapper.ToObject<List<Item.Data>>(json2Split);


        foreach (var item in items1To2)
        {
            bool found = false;
            for (int i = 0; i < character1.inventory.Count; i++)
            {
                if (character1.inventory[i].networkID == item.item.networkID)
                {
                    character1.inventory[i].SetAmount(item.item.value);

                    Item merge = null;
                    if (item.merge && string.IsNullOrEmpty(item.mergeID) == false)
                    {
                        for (int j = 0; j < character2.inventory.Count; j++)
                        {
                            if (character2.inventory[j].networkID == item.mergeID)
                            {
                                merge = character2.inventory[j];
                                break;
                            }
                        }
                        if (merge == null)
                        {
                            // Problem
                        }
                    }

                    character2.AddItemToInventoryLocally(character1.inventory[i], merge);
                    character1.RemoveItemFromInventoryLocally(character1.inventory[i]);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                // Problem
            }
        }

        foreach (var item in splitItems1)
        {
            Item prefab = PrefabManager.singleton.GetItemPrefab(item.id);
            if (prefab != null)
            {
                Item splitItem = Instantiate(prefab, transform);
                splitItem.networkID = item.networkID;
                splitItem.SetAmount(item.value);
                character1.AddItemToInventoryLocally(splitItem);
            }
        }

        foreach (var item in items2To1)
        {
            bool found = false;
            for (int i = 0; i < character2.inventory.Count; i++)
            {
                if (character2.inventory[i].networkID == item.item.networkID)
                {
                    character2.inventory[i].SetAmount(item.item.value);

                    Item merge = null;
                    if (item.merge && string.IsNullOrEmpty(item.mergeID) == false)
                    {
                        for (int j = 0; j < character1.inventory.Count; j++)
                        {
                            if (character1.inventory[j].networkID == item.mergeID)
                            {
                                merge = character1.inventory[j];
                                break;
                            }
                        }
                        if (merge == null)
                        {
                            // Problem
                        }
                    }

                    character1.AddItemToInventoryLocally(character2.inventory[i], merge);
                    character2.RemoveItemFromInventoryLocally(character2.inventory[i]);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                // Problem
            }
        }

        foreach (var item in splitItems2)
        {
            Item prefab = PrefabManager.singleton.GetItemPrefab(item.id);
            if (prefab != null)
            {
                Item splitItem = Instantiate(prefab, transform);
                splitItem.networkID = item.networkID;
                splitItem.SetAmount(item.value);
                character2.AddItemToInventoryLocally(splitItem);
            }
        }
    }

    public void UpdateItemPosition(Item item)
    {
        if (item != null)
        {
            Item.Data data = item.GetData();
            string json = JsonMapper.ToJson(data);
            UpdateItemPositionClientRpc(json);
        }
    }

    [ClientRpc]
    private void UpdateItemPositionClientRpc(string itemJson)
    {
        Item.Data data = JsonMapper.ToObject<Item.Data>(itemJson);
        Item[] allItems = FindObjectsByType<Item>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (allItems != null)
        {
            for (int i = 0; i < allItems.Length; i++)
            {
                if (allItems[i].networkID == data.networkID)
                {
                    allItems[i].transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
                    allItems[i].transform.eulerAngles = new Vector3(data.rotation[0], data.rotation[1], data.rotation[2]);
                    break;
                }
            }
        }
    }

}