using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �J�[�h�̎���
/// </summary>
public class CardModel
{
    public Sprite icon { get; private set; }
    public string name {  get; private set; }
    public int cost { get; private set; }
    public int atk { get; private set; }
    public int hp {  get; private set; }
    
    public CardEntity.CategoryRarity categoryRarity { get; private set; }
    public CardEntity.skill skill1 { get; private set; }
    public CardEntity.skill skill2 { get; private set; }
    public CardEntity.skill skill3 { get; private set; }
    public string cardText { get; private set; }
    public bool isPlayerCard { get; private set; }
    public bool isFieldCard { get; private set; }
    public int fieldID { get; private set; }
    public bool isAlive {  get; private set; }
    public bool canAttack {  get; private set; }
    public bool is���� { get; private set; }
    
    public CardModel(int cardID, bool isPlayer)
    {
        //cardID����ɑΏۂ̃J�[�h�f�[�^���擾����
        CardEntity cardEntity = Resources.Load<CardEntity>($"CardEntityList/Card{cardID}");
        icon = cardEntity.icon;
        name = cardEntity.name;
        cost = cardEntity.cost;
        atk = cardEntity.atk;
        hp = cardEntity.hp;
        skill1 = cardEntity.skill1;
        skill2 = cardEntity.skill2;
        skill3 = cardEntity.skill3;
        cardText = cardEntity.cardText;
        categoryRarity = cardEntity.categoryRarity; 
        isPlayerCard = isPlayer;
        isFieldCard = false;
        fieldID = 0;
        isAlive = true;
        canAttack = false;
        is���� = false;
    }
    /// <summary>
    /// �J�[�h���_���[�W���󂯂����̏���
    /// </summary>
    /// <param name="dmg"></param>
    void Damage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            hp = 0;
            isAlive = false;
        }
    }
    /// <summary>
    /// view���̏���������̂ŁACC�ȊO���璼�ڌĂԂ͔̂񐄏�
    /// </summary>
    public void Attack(CardController card)
    {
        card.model.Damage(atk);
    }
    /// <summary>
    /// view���̏���������̂ŁACC�ȊO���璼�ڌĂԂ͔̂񐄏�
    /// </summary>
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    /// <summary>
    /// view���̏���������̂ŁACC�ȊO���璼�ڌĂԂ͔̂񐄏�
    /// </summary>
    /// <param name="isFieldCard"></param>
    public void SetIsFieldCard(bool isFieldCard) { 
        this.isFieldCard = isFieldCard;
    }
    public void SetIsFieldID(int fieldID)
    {
        this.fieldID = fieldID;
    }
    public void SetIs����(bool is����)
    {
        this.is���� = is����;
    }
}
