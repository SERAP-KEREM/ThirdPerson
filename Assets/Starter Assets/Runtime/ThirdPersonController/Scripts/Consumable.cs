using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : Item
{

    [Header("Consumable")]
    [SerializeField] private Action _action = Action.Heal; public Action action { get { return _action; } }
    [SerializeField] private float _actionValue = 1; public float actionValue { get { return _actionValue; } }

    public enum Action
    {
        Heal
    }

    private int _amount = 0; public int amount { get { return _amount; } set { _amount = value; } }

    public void Consume(Character character)
    {
        if (_amount > 0 && character != null)
        {

        }
    }

}