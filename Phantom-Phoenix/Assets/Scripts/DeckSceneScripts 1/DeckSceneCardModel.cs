using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// カードの実体
/// </summary>
public class DeckSceneCardModel
{
    public int cardID {  get; private set; }
    public Sprite icon { get; private set; }
    public string name {  get; private set; }
    public int cost { get; private set; }
    public int atk { get; private set; }
    public int maxHP { get; private set; }
    public int hp {  get; private set; }
    
    public CardEntity.Category category{ get; private set; }
    public CardEntity.Rarity rarity { get; private set; }
    public CardEntity.SpellTarget spellTarget { get; private set; }
    public CardEntity.Skill skill1 { get; private set; }
    public CardEntity.Skill skill2 { get; private set; }
    public CardEntity.Skill skill3 { get; private set; }
    public string cardText { get; private set; }

    public DeckSceneCardModel(int cardID, bool isPlayer)
    {
        //cardIDを基に対象のカードデータを取得する
        CardEntity cardEntity = Resources.Load<CardEntity>($"CardEntityList/Card{cardID}");
        this.cardID = cardID;
        icon = cardEntity.icon;
        name = cardEntity.name;
        cost = cardEntity.cost;
        atk = cardEntity.atk;
        maxHP = hp = cardEntity.hp;
        category = cardEntity.category;
        spellTarget = cardEntity.spellTarget;
        rarity = cardEntity.rarity;
        skill1 = cardEntity.skill1;
        skill2 = cardEntity.skill2;
        skill3 = cardEntity.skill3;
        cardText = cardEntity.cardText;
    }
}
