using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// カードの実体
/// </summary>
public class CardModel
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
    public bool isPlayerCard { get; private set; }
    public bool isFieldCard { get; private set; }
    public int fieldID { get; private set; }
    public bool isAlive {  get; private set; }
    public bool canAttack {  get; private set; }
    public bool isTaunt { get; private set; }
    public bool isActiveDoubleAction {  get; private set; }
    public bool HasSelectSpeciallSkill { get; private set; }

    public CardModel(int cardID, bool isPlayer)
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
        isPlayerCard = isPlayer;
        isFieldCard = false;
        fieldID = 0;
        isAlive = true;
        canAttack = false;
        isTaunt = false;
        isActiveDoubleAction = true;
        if (spellTarget == CardEntity.SpellTarget.enemyUnit)
        {
            HasSelectSpeciallSkill = true;
        }
        else
        {
            HasSelectSpeciallSkill = false;
        }
    }
    /// <summary>
    /// カードがダメージを受けた時の処理
    /// </summary>
    /// <param name="dmg"></param>
    public void Damage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            hp = 0;
            isAlive = false;
        }
    }
    /// <summary>
    /// カードが回復を受けた時の処理
    /// </summary>
    /// <param name="dmg"></param>
    public void Heal(int hl)
    {
        hp += hl;
        if (hp > maxHP)
        {
            hp = maxHP;
        }
    }
    /// <summary>
    /// view側の処理があるので、CC以外から直接呼ぶのは非推奨
    /// </summary>
    public void Attack<T>(T card) where T : Controller
    {
        if (card is CardController cc)
        {
            cc.DamageFromAttack(atk);
        }
        else if (card is HeroController hc)
        {
            hc.Damage(atk);
        }
    }
    /// <summary>
    /// view側の処理があるので、CC以外から直接呼ぶのは非推奨
    /// </summary>
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    /// <summary>
    /// view側の処理があるので、CC以外から直接呼ぶのは非推奨
    /// </summary>
    /// <param name="isFieldCard"></param>
    public void SetIsFieldCard(bool isFieldCard) { 
        this.isFieldCard = isFieldCard;
    }
    public void SetIsFieldID(int fieldID)
    {
        this.fieldID = fieldID;
    }
    public void SetisTaunt(bool isTaunt)
    {
        this.isTaunt = isTaunt;
    }
    public void SetisActiveDoubleAction(bool isActiveDoubleAction)
    {
        this.isActiveDoubleAction = isActiveDoubleAction;
    }
}
