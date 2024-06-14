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
    [NonSerialized]
    public bool isFieldCard;
    [NonSerialized]
    public bool isPlayerCard;
    public CardModel(int cardID, bool isPlayer)
    {
        CardEntity cardEntity = Resources.Load<CardEntity>($"CardEntityList/Card{cardID}");
        name = cardEntity.name;
        hp = cardEntity.hp;
        atk = cardEntity.atk;
        cost = cardEntity.cost;
        icon = cardEntity.icon;
        isAlive = true;
        canAttack = false;
        isFieldCard = false;
        isPlayerCard = isPlayer;
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
