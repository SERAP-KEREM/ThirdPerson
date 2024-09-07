using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CanvasManager : MonoBehaviour
{

    [Header("Pickup Box")]
    [SerializeField] public GameObject _itemPickupPanel = null;
    [SerializeField] public RectTransform _itemPickupBox = null;
    [SerializeField] public TextMeshProUGUI _itemPickupName = null;
    [SerializeField] public TextMeshProUGUI _itemPickupAmount = null;
    [SerializeField] public float _rightOffset = 5f;
    [SerializeField] public float _leftOffset = 5f;
    [SerializeField] public float _topOffset = 5f;
    [SerializeField] public float _buttomOffset = 5f;

    [Header("Loot Box")]
    [SerializeField] public GameObject _itemLootPanel = null;
    [SerializeField] public RectTransform _itemLootBox = null;
    [SerializeField] public TextMeshProUGUI _itemLootName = null;
    [SerializeField] public float _itemLootRightOffset = 5f;
    [SerializeField] public float _itemLootLeftOffset = 5f;
    [SerializeField] public float _itemLootTopOffset = 5f;
    [SerializeField] public float _itemLootButtomOffset = 5f;

    [Header("Inventory")]
    [SerializeField] public GameObject _inventoryPanel = null;
    [SerializeField] private Button _inventoryCloseButton = null;
    [SerializeField] public InventoryItem _inventoryItemPrefab = null;
    [SerializeField] public RectTransform _inventoryGrid1 = null;
    [SerializeField] public RectTransform _inventoryGrid2 = null;
    [SerializeField] public TextMeshProUGUI _inventoryGridTitle1 = null;
    [SerializeField] public TextMeshProUGUI _inventoryGridTitle2 = null;

    private static CanvasManager _singleton = null;
    public static CanvasManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<CanvasManager>();
            }
            return _singleton;
        }
    }

    private Item _itemToPick = null; public Item itemToPick { get { return _itemToPick; } set { _itemToPick = value; OnItemToPickUpdated(); } }
    private Character _characterToLoot = null; public Character characterToLoot { get { return _characterToLoot; } set { _characterToLoot = value; OnCharacterToLootUpdated(); } }
    private Character _characterLootTarget = null;

    private Vector2 _referenceResolution = new Vector2(1920, 1080);
    private Vector2 _screenScale = new Vector2(1, 1);

    private List<InventoryItem> _inventoryItems1 = new List<InventoryItem>();
    private List<InventoryItem> _inventoryItems2 = new List<InventoryItem>();

    private bool _isInventoryOpen = false; public bool isInventoryOpen { get { return _isInventoryOpen; } }

    private void Awake()
    {
        _itemPickupPanel.gameObject.SetActive(false);
        _inventoryPanel.gameObject.SetActive(false);
        _itemLootPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        _inventoryCloseButton.onClick.AddListener(CloseInventory);

        _itemPickupBox.anchorMax = Vector2.zero;
        _itemPickupBox.anchorMin = Vector2.zero;
        _itemPickupBox.pivot = Vector2.zero;

        _itemLootBox.anchorMax = Vector2.zero;
        _itemLootBox.anchorMin = Vector2.zero;
        _itemLootBox.pivot = Vector2.zero;

        CanvasScaler _scaler = GetComponent<CanvasScaler>();
        if (_scaler != null)
        {
            _referenceResolution = _scaler.referenceResolution;
            _screenScale = new Vector2(_referenceResolution.x / Screen.width, _referenceResolution.y / Screen.height);
        }
    }

    private void Update()
    {
        if (_itemToPick != null)
        {
            Vector2 position = CameraManager1.mainCamera.WorldToScreenPoint(_itemToPick.transform.position) * _screenScale;
            if (position.x - _leftOffset < 0)
            {
                position.x = _leftOffset;
            }
            if (position.x + _itemPickupBox.sizeDelta.x + _rightOffset > _referenceResolution.x)
            {
                position.x = _referenceResolution.x - _itemPickupBox.sizeDelta.x - _rightOffset;
            }
            if (position.y - _buttomOffset < 0)
            {
                position.y = _buttomOffset;
            }
            if (position.y + _itemPickupBox.sizeDelta.y + _topOffset > _referenceResolution.y)
            {
                position.y = _referenceResolution.y - _itemPickupBox.sizeDelta.y - _topOffset;
            }
            _itemPickupBox.anchoredPosition = position;
        }
        else if (_characterToLoot != null)
        {
            Vector2 position = CameraManager1.mainCamera.WorldToScreenPoint(_characterToLoot.transform.position) * _screenScale;
            if (position.x - _itemLootLeftOffset < 0)
            {
                position.x = _itemLootLeftOffset;
            }
            if (position.x + _itemLootBox.sizeDelta.x + _itemLootRightOffset > _referenceResolution.x)
            {
                position.x = _referenceResolution.x - _itemLootBox.sizeDelta.x - _itemLootRightOffset;
            }
            if (position.y - _itemLootButtomOffset < 0)
            {
                position.y = _itemLootButtomOffset;
            }
            if (position.y + _itemLootBox.sizeDelta.y + _itemLootTopOffset > _referenceResolution.y)
            {
                position.y = _referenceResolution.y - _itemLootBox.sizeDelta.y - _itemLootTopOffset;
            }
            _itemLootBox.anchoredPosition = position;
        }
    }

    private void OnItemToPickUpdated()
    {
        if (_itemToPick != null)
        {
            _itemPickupName.text = _itemToPick.id;
            _itemPickupAmount.text = "x" + _itemToPick.GetAmount().ToString();
            _itemPickupPanel.gameObject.SetActive(true);
        }
        else
        {
            _itemPickupPanel.gameObject.SetActive(false);
        }
    }

    private void OnCharacterToLootUpdated()
    {
        if (_characterToLoot != null)
        {
            _itemLootName.text = _characterToLoot.id;
            _itemLootPanel.gameObject.SetActive(true);
        }
        else
        {
            _itemLootPanel.gameObject.SetActive(false);
        }
    }

    public void CloseInventory()
    {
        if (_isInventoryOpen == false)
        {
            return;
        }

        if (_characterLootTarget != null)
        {
            if (Character.localPlayer != null)
            {
                Dictionary<Item, int> itemsToStore = new Dictionary<Item, int>();
                Dictionary<Item, int> itemsToTake = new Dictionary<Item, int>();
                for (int i = 0; i < _inventoryItems2.Count; i++)
                {
                    if (_inventoryItems2[i] != null && _inventoryItems2[i].item != null && Character.localPlayer.inventory.Contains(_inventoryItems2[i].item))
                    {
                        itemsToStore.Add(_inventoryItems2[i].item, _inventoryItems2[i].count);
                    }
                }
                for (int i = 0; i < _inventoryItems1.Count; i++)
                {
                    if (_inventoryItems1[i] != null && _inventoryItems1[i].item != null && _characterLootTarget.inventory.Contains(_inventoryItems1[i].item))
                    {
                        itemsToTake.Add(_inventoryItems1[i].item, _inventoryItems1[i].count);
                    }
                }
                if (itemsToStore.Count > 0 || itemsToTake.Count > 0)
                {
                    SessionManager.singleton.TradeItemsBetweenCharacters(Character.localPlayer, _characterLootTarget, itemsToStore, itemsToTake);
                }
            }
        }
        else
        {
            if (_inventoryItems2.Count > 0 && Character.localPlayer != null)
            {
                Dictionary<Item, int> items = new Dictionary<Item, int>();
                for (int i = 0; i < _inventoryItems2.Count; i++)
                {
                    if (_inventoryItems2[i] != null && _inventoryItems2[i].item != null)
                    {
                        items.Add(_inventoryItems2[i].item, _inventoryItems2[i].count);
                    }
                }
                Character.localPlayer.DropItems(items);
            }
        }

        _characterLootTarget = null;
        _isInventoryOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        _inventoryPanel.gameObject.SetActive(false);
    }

    public void OpenInventory()
    {
        if (_isInventoryOpen)
        {
            return;
        }
        if (Character.localPlayer != null)
        {
            ClearInventoryItems();
            _inventoryGridTitle1.text = "Inventory";
            _inventoryGridTitle2.text = "On Ground";
            for (int i = 0; i < Character.localPlayer.inventory.Count; i++)
            {
                if (Character.localPlayer.inventory[i].GetType() != typeof(Weapon) && Character.localPlayer.inventory[i].GetAmount() <= 0)
                {
                    continue;
                }
                InventoryItem item = Instantiate(_inventoryItemPrefab, _inventoryGrid1);
                item.Initialize(Character.localPlayer.inventory[i]);
                _inventoryItems1.Add(item);
            }
            _isInventoryOpen = true;
            Cursor.lockState = CursorLockMode.None;
            _inventoryPanel.gameObject.SetActive(true);
        }
    }

    public void OpenInventoryForLoot(Character lootTarget)
    {
        if (_isInventoryOpen)
        {
            return;
        }
        if (lootTarget != null && lootTarget.health <= 0 && Character.localPlayer != null && lootTarget != Character.localPlayer)
        {
            _characterLootTarget = lootTarget;
            ClearInventoryItems();
            _inventoryGridTitle1.text = "Inventory";
            // ToDo: Use username
            _inventoryGridTitle2.text = "Player" + lootTarget.clientID.ToString();
            for (int i = 0; i < Character.localPlayer.inventory.Count; i++)
            {
                if (Character.localPlayer.inventory[i].GetType() != typeof(Weapon) && Character.localPlayer.inventory[i].GetAmount() <= 0)
                {
                    continue;
                }
                InventoryItem item = Instantiate(_inventoryItemPrefab, _inventoryGrid1);
                item.Initialize(Character.localPlayer.inventory[i]);
                _inventoryItems1.Add(item);
            }
            for (int i = 0; i < _characterLootTarget.inventory.Count; i++)
            {
                if (_characterLootTarget.inventory[i].GetType() != typeof(Weapon) && _characterLootTarget.inventory[i].GetAmount() <= 0)
                {
                    continue;
                }
                InventoryItem item = Instantiate(_inventoryItemPrefab, _inventoryGrid2);
                item.Initialize(_characterLootTarget.inventory[i]);
                _inventoryItems2.Add(item);
            }
            _isInventoryOpen = true;
            Cursor.lockState = CursorLockMode.None;
            _inventoryPanel.gameObject.SetActive(true);
        }
    }

    public void ItemClicked(InventoryItem item)
    {
        if (item != null && item.item != null)
        {
            if (_inventoryItems1.Contains(item))
            {
                _inventoryItems1.Remove(item);
                item.transform.SetParent(_inventoryGrid2);
                _inventoryItems2.Add(item);
            }
            else if (_inventoryItems2.Contains(item))
            {
                item.transform.SetParent(_inventoryGrid1);
                _inventoryItems2.Remove(item);
                _inventoryItems1.Add(item);
            }
        }
    }

    private void ClearInventoryItems()
    {
        for (int i = 0; i < _inventoryItems1.Count; i++)
        {
            if (_inventoryItems1[i] != null)
            {
                Destroy(_inventoryItems1[i].gameObject);
            }
        }
        for (int i = 0; i < _inventoryItems2.Count; i++)
        {
            if (_inventoryItems2[i] != null)
            {
                Destroy(_inventoryItems2[i].gameObject);
            }
        }
        _inventoryItems1.Clear();
        _inventoryItems2.Clear();
    }

}