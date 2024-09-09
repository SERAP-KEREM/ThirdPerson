using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevelopersHub.RealtimeNetworking.Client;
using TMPro;
using UnityEngine.UI;

public class MenuWeaponItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _nameText = null;
    [SerializeField] public Button _button = null;

    [HideInInspector] public Data.RuntimeEquipment weaponData = null;

    private void Start()
    {
        _button.onClick.AddListener(Clicked);
    }

    public void Initialize(Data.RuntimeEquipment data)
    {
        weaponData = data;
        _nameText.text = data.tag;
    }

    private void Clicked()
    {
        CharactersManager.singleton.WeaponItemClicked(this);
    }

}