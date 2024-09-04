using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI _itemName = null;
    [SerializeField] public TextMeshProUGUI _itemAmount = null;

    private Item _item = null; public Item item { get { return _item; } }
    private int _count = 1; public int count { get { return _count; } set { _count = value; } }

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(Clicked);
        }
    }

    public void Initialize(Item item)
    {
        if (item != null)
        {
            _item = item;
            _itemName.text = _item.id;
            _count = 1;
            if (item.GetType() == typeof(Weapon))
            {
                _itemAmount.text = "x" + ((Weapon)item).ammo.ToString();
                _count = ((Weapon)item).ammo;
            }
            else if (item.GetType() == typeof(Ammo))
            {
                _itemAmount.text = "x" + ((Ammo)item).amount.ToString();
                _count = ((Ammo)item).amount;
            }
        }
    }

    private void Clicked()
    {
        CanvasManager.singleton.ItemClicked(this);
    }

}