using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevelopersHub.RealtimeNetworking.Client;
using TMPro;
using UnityEngine.UI;

public class MenuCharacterItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _nameText = null;
    [SerializeField] public Button _button = null;

    [HideInInspector] public Data.RuntimeCharacter characterData = null;

    private void Start()
    {
        _button.onClick.AddListener(Clicked);
    }

    public void Initialize(Data.RuntimeCharacter data)
    {
        characterData = data;
        _nameText.text = data.tag;
    }

    private void Clicked()
    {
        CharactersManager.singleton.CharacterItemClicked(this);
    }

    public void SetSelectStatus(bool status)
    {

    }

}