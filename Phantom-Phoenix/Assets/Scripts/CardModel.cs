using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardModel
{
    public string name;
    public int hp;
    public int atk;
    public int cost;
    public Sprite icon;
    [NonSerialized]
    public bool isAlive;
    [NonSerialized]
    public bool canAttack;

    public CardModel(int cardID)
    {
        CardEntity cardEntity = Resources.Load<CardEntity>($"CardEntityList/Card{cardID}");
        name = cardEntity.name;
        hp = cardEntity.hp;
        atk = cardEntity.atk;
        cost = cardEntity.cost;
        icon = cardEntity.icon;
        isAlive = true;
        canAttack = false;
    }

    void Damage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            hp = 0;
            isAlive = false;
        }
    }
    public void Attack(CardController card)
    {
        card.model.Damage(atk);
    }
}
