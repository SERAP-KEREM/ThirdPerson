using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Button _serverButton = null;
    [SerializeField] private Button _clientButton = null;

    [Header("Pickup Box")]
    [SerializeField] public GameObject _itemPickupPanel = null;
    [SerializeField] public RectTransform _itemPickupBox = null;
    [SerializeField] public TextMeshProUGUI _itemPickupName = null;
    [SerializeField] public TextMeshProUGUI _itemPickupAmount = null;
    [SerializeField] public float _rightOffset = 5f;
    [SerializeField] public float _leftOffset = 5f;
    [SerializeField] public float _topOffset = 5f;
    [SerializeField] public float _buttomOffset = 5f;


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

    private Vector2 _referenceResolution = new Vector2(1920, 1080);
    private Vector2 _screenScale = new Vector2(1, 1);

  private void Awake()
    {
        _itemPickupPanel.gameObject.SetActive(false);

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        _serverButton.onClick.AddListener(StartServer);
        _clientButton.onClick.AddListener(StartClient);
        _itemPickupBox.anchorMax = Vector2.zero;
        _itemPickupBox.anchorMin = Vector2.zero;
        _itemPickupBox.pivot = Vector2.zero;

        CanvasScaler _scaler = GetComponent<CanvasScaler>();
        if (_scaler != null)
        {
            _referenceResolution = _scaler.referenceResolution;
            _screenScale = new Vector2(_referenceResolution.x / Screen.width, _referenceResolution.y / Screen.height);
        }
    }

    private void StartServer()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _serverButton.gameObject.SetActive(false);
        _clientButton.gameObject.SetActive(false);
        SessionManager.singleton.StartServer();
    }

    private void StartClient()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _serverButton.gameObject.SetActive(false);
        _clientButton.gameObject.SetActive(false);
        SessionManager.singleton.StartClient();
    }
    void Update()
    {
        // Eğer ESC tuşuna basılırsa, mouse tekrar görünür hale gelir.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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
    }
    private void OnItemToPickUpdated()
    {
        if (_itemToPick != null)
        {
            _itemPickupName.text = _itemToPick.id;
            if (_itemToPick.GetType() == typeof(Ammo))
            {
                _itemPickupAmount.text = "x" + ((Ammo)_itemToPick).amount.ToString();

            }
            else
            {
                _itemPickupAmount.text = "x1";

            }
            _itemPickupPanel.gameObject.SetActive(true);

        }
        else
        {
            _itemPickupPanel.gameObject.SetActive(false);

        }
    }


}
