using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevelopersHub.RealtimeNetworking.Client;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;

public class CharactersManager : MonoBehaviour
{

    [SerializeField] private RectTransform _panel = null;
    [SerializeField] private RectTransform _panelWeapons = null;
    [SerializeField] private Transform _spawnPoint = null;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private MenuCharacterItem _characterItemPrefab = null;
    [SerializeField] private RectTransform _characterItemParent = null;
    [SerializeField] private MenuWeaponItem _weaponItemPrefab = null;
    [SerializeField] private RectTransform _weaponItemParent = null;
    [SerializeField] public Button _back = null;
    [SerializeField] public Button _changeWeapon = null;
    [SerializeField] public Button _setActive = null;
    [SerializeField] public Button _changeWeaponBack = null;
    [SerializeField] public Button _unequpWeapon = null;

    private static CharactersManager _singleton = null;
    public static CharactersManager singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<CharactersManager>(FindObjectsInactive.Include);
            }
            return _singleton;
        }
    }

    private List<MenuCharacterItem> _characterItems = new List<MenuCharacterItem>();
    private MenuCharacterItem _selectedCharacter = null;
    private List<Data.RuntimeCharacter> _characters = new List<Data.RuntimeCharacter>();
    private Character _character = null;
    private bool _buzy = false;
    private List<MenuWeaponItem> _weaponItems = new List<MenuWeaponItem>();
    private MenuWeaponItem _weaponToEquip = null;

    private void Awake()
    {
        Close();
        RealtimeNetworking.OnGetCharacters += OnGetCharacters;
        RealtimeNetworking.OnSetCharacterSelected += OnCharacterSelected;
        RealtimeNetworking.OnUnquipCharacterEquipment += OnUnquipCharacterEquipment;
        RealtimeNetworking.OnEquipCharacterEquipment += OnEquipCharacterEquipment;
        RealtimeNetworking.OnGetEquipments += OnGetEquipments;

        _back.onClick.AddListener(Close);
        _changeWeapon.onClick.AddListener(ChangeWeapon);
        _setActive.onClick.AddListener(SetAsActive);
        _changeWeaponBack.onClick.AddListener(ChangeWeaponBack);
        _unequpWeapon.onClick.AddListener(UnequipWeapon);
    }

    private void OnDestroy()
    {
        RealtimeNetworking.OnGetCharacters -= OnGetCharacters;
        RealtimeNetworking.OnSetCharacterSelected -= OnCharacterSelected;
        RealtimeNetworking.OnUnquipCharacterEquipment -= OnUnquipCharacterEquipment;
        RealtimeNetworking.OnEquipCharacterEquipment -= OnEquipCharacterEquipment;
        RealtimeNetworking.OnGetEquipments -= OnGetEquipments;
    }

    private void OnGetEquipments(RealtimeNetworking.GeneralResponse response, List<Data.RuntimeEquipment> equipment)
    {
        if (response == RealtimeNetworking.GeneralResponse.SUCCESSFULL)
        {
            for (int i = 0; i < equipment.Count; i++)
            {
                MenuWeaponItem weaponItem = Instantiate(_weaponItemPrefab, _weaponItemParent);
                weaponItem.Initialize(equipment[i]);
                _weaponItems.Add(weaponItem);
            }
        }
        _buzy = false;
    }

    private void OnEquipCharacterEquipment(RealtimeNetworking.SetEquipmentStatusResponse response)
    {
        if (response == RealtimeNetworking.SetEquipmentStatusResponse.SUCCESSFULL && _selectedCharacter != null)
        {
            _selectedCharacter.characterData.equipments.Clear();
            _selectedCharacter.characterData.equipments.Add(_weaponToEquip.weaponData);
            ShowCharacter(_selectedCharacter.characterData);
        }
        _buzy = false;
        _weaponToEquip = null;
    }

    private void OnUnquipCharacterEquipment(RealtimeNetworking.SetEquipmentStatusResponse response)
    {
        if (response == RealtimeNetworking.SetEquipmentStatusResponse.SUCCESSFULL)
        {
            if (_selectedCharacter != null)
            {
                _selectedCharacter.characterData.equipments.Clear();
                ShowCharacter(_selectedCharacter.characterData);
                _panelWeapons.gameObject.SetActive(false);
            }
        }
        _buzy = false;
    }

    private void OnCharacterSelected(RealtimeNetworking.SetCharacterSelectedResponse response)
    {
        if (response == RealtimeNetworking.SetCharacterSelectedResponse.SUCCESSFULL)
        {
            _setActive.gameObject.SetActive(false);
            for (int i = 0; i < _characterItems.Count; i++)
            {
                if (_characterItems[i] == _selectedCharacter)
                {
                    _characterItems[i].characterData.selected = true;
                }
                else
                {
                    _characterItems[i].characterData.selected = false;
                }
            }
        }
        _buzy = false;
    }

    private void OnGetCharacters(RealtimeNetworking.GeneralResponse response, List<Data.RuntimeCharacter> characters)
    {
        if (response == RealtimeNetworking.GeneralResponse.SUCCESSFULL)
        {
            _characters = characters;
            for (int i = 0; i < _characters.Count; i++)
            {
                MenuCharacterItem item = Instantiate(_characterItemPrefab, _characterItemParent);
                item.Initialize(_characters[i]);
                _characterItems.Add(item);
                if (_characters[i].selected && _character == null)
                {
                    ShowCharacter(_characters[i]);
                    _selectedCharacter = item;
                    item.SetSelectStatus(true);
                }
            }
        }
        _buzy = false;
    }

    public void WeaponItemClicked(MenuWeaponItem item)
    {
        if (item == null || _selectedCharacter == null || _buzy)
        {
            return;
        }
        _buzy = true;
        _weaponToEquip = item;
        RealtimeNetworking.EquipCharacterEquipment(_selectedCharacter.characterData.id, item.weaponData.id, true);
        _panelWeapons.gameObject.SetActive(false);
    }

    public void CharacterItemClicked(MenuCharacterItem item)
    {
        if (_buzy || item == null || _selectedCharacter == item || item.characterData == null)
        {
            return;
        }
        if (_selectedCharacter != null)
        {
            _selectedCharacter.SetSelectStatus(false);
        }
        _selectedCharacter = item;
        _selectedCharacter.SetSelectStatus(true);
        ShowCharacter(_selectedCharacter.characterData);
    }

    public void Open()
    {
        _camera.enabled = true;
        ClearCharacterItems();
        RealtimeNetworking.GetCharacters(RealtimeNetworking.accountID, false, true);
        _panel.transform.SetAsLastSibling();
        _panelWeapons.gameObject.SetActive(false);
        _panel.gameObject.SetActive(true);
    }

    public void Close()
    {
        _camera.enabled = false;
        ClearCharacterItems();
        _panel.gameObject.SetActive(false);
    }

    private void ClearCharacterItems()
    {
        for (int i = 0; i < _characterItems.Count; i++)
        {
            if (_characterItems[i] != null)
            {
                Destroy(_characterItems[i].gameObject);
            }
        }
        _characterItems.Clear();
    }

    private void ClearWeaponItems()
    {
        for (int i = 0; i < _weaponItems.Count; i++)
        {
            if (_weaponItems[i] != null)
            {
                Destroy(_weaponItems[i].gameObject);
            }
        }
        _weaponItems.Clear();
    }

    private void ShowCharacter(Data.RuntimeCharacter character)
    {
        if (character == null)
        {
            return;
        }
        if (_character != null)
        {
            Destroy(_character.gameObject);
        }
        _setActive.gameObject.SetActive(character.selected == false);
        Character prefab = PrefabManager.singleton.GetCharacterPrefab(character.tag);
        if (prefab != null)
        {
            _character = Instantiate(prefab, _spawnPoint.position, _spawnPoint.rotation);
            CharacterController _controller = _character.GetComponent<CharacterController>();
            StarterAssetsInputs _input = _character.GetComponent<StarterAssetsInputs>();
            PlayerInput _playerInput = _character.GetComponent<PlayerInput>();
            ThirdPersonController _tps = _character.GetComponent<ThirdPersonController>();
            if (_tps != null)
            {
                Destroy(_tps);
            }
            if (_playerInput != null)
            {
                Destroy(_playerInput);
            }
            if (_input != null)
            {
                Destroy(_input);
            }
            if (_controller != null)
            {
                Destroy(_controller);
            }
            if (character.equipments.Count > 0)
            {
                for (int i = 0; i < character.equipments.Count; i++)
                {
                    Item equipmentPrefab = PrefabManager.singleton.GetItemPrefab(character.equipments[i].tag);
                    if (equipmentPrefab != null && equipmentPrefab.GetType() == typeof(Weapon))
                    {
                        _character.InitializeDummy(character.equipments[i].tag);
                        break;
                    }
                }
            }
        }
    }

    private void SetAsActive()
    {
        if (_selectedCharacter != null && _buzy == false)
        {
            _buzy = true;
            RealtimeNetworking.SetCharacterSelectedStaus(_selectedCharacter.characterData.id, true, true);
        }
    }

    private void ChangeWeapon()
    {
        if (_selectedCharacter != null && _buzy == false)
        {
            _buzy = true;
            ClearWeaponItems();
            _panelWeapons.gameObject.SetActive(true);
            RealtimeNetworking.GetEquipments(RealtimeNetworking.accountID, true);
        }
    }

    private void ChangeWeaponBack()
    {
        _panelWeapons.gameObject.SetActive(false);
    }

    private void UnequipWeapon()
    {
        if (_selectedCharacter != null && _selectedCharacter.characterData.equipments.Count > 0 && _buzy == false)
        {
            _buzy = true;
            _weaponToEquip = null;
            RealtimeNetworking.UnquipCharacterEquipment(_selectedCharacter.characterData.id, _selectedCharacter.characterData.equipments[0].id);
        }
    }

}