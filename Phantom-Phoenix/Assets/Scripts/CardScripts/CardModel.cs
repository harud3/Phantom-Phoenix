using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// �J�[�h�̎��� Card�v���n�u�ɂ��Ă�
/// </summary>
public class CardModel
{
    public int cardID { get; private set; }
    public Sprite character { get; private set; }
    public string name { get; private set; }

    public int cost { get; private set; }
    public int defaultATK { get; private set; }
    public int atk { get; private set; }
    public int defaultHP { get; private set; }
    public int maxHP { get; private set; }
    public int hp { get; private set; }

    public CardEntity.Category category { get; private set; }
    public CardEntity.Rarity rarity { get; private set; }

    public CardEntity.Skill skill1 { get; private set; }
    public CardEntity.Skill skill2 { get; private set; }
    public CardEntity.Skill skill3 { get; private set; }
    public string textCard { get; private set; }
    public CardEntity.Target target { get; private set; }

    public bool isMulliganCard { get; private set; }
    public bool isMulligan { get; private set; }

    public bool isPlayerCard { get; private set; }
    public bool isFieldCard { get; private set; }
    public int thisFieldID { get; private set; }
    public bool isAlive { get; private set; }
    public bool isSummonThisTurn { get; private set; }
    public bool canAttack { get; private set; }
    public bool isTaunt { get; private set; }
    /// <summary>
    /// �A���� ���ꂪtrue�Ȃ�A�U������canAttack��false�ɂ����A����ɂ����false�ɂ���@�U�����̍s�������1��Ȃ��������Ƃɂ���
    /// </summary>
    public bool isActiveDoubleAction { get; private set; }

    public bool HasSelectSpeciallSkill { get; private set; }
    public bool isSeal { get; private set; }
    public CardModel(int cardID, bool isPlayer)
    {
        //cardID����ɑΏۂ̃J�[�h�f�[�^���擾����
        CardEntity cardEntity = Resources.Load<CardEntity>($"CardEntityList/Card{cardID}");

        this.cardID = cardID;
        character = cardEntity.charcter;
        name = cardEntity.name;

        cost = cardEntity.cost;
        defaultATK = atk = cardEntity.atk;
        Debug.Log(defaultATK);
        defaultHP = maxHP = hp = cardEntity.hp;

        category = cardEntity.category;
        rarity = cardEntity.rarity;

        skill1 = cardEntity.skill1;
        skill2 = cardEntity.skill2;
        skill3 = cardEntity.skill3;
        textCard = cardEntity.textCard;
        target = cardEntity.target;

        isMulliganCard = isMulligan = false;
        isPlayerCard = isPlayer;
        isFieldCard = false;
        thisFieldID = 0;
        isAlive = true;
        isSummonThisTurn = true;
        canAttack = false;

        isTaunt = false;
        isActiveDoubleAction = true;
        //�I�����K�v�ȏꍇ�A���̃t���O��true�ɂ��ĊǗ�����
        if (target == CardEntity.Target.unit || target == CardEntity.Target.enemyUnit)
        {
            HasSelectSpeciallSkill = true;
        }
        else
        {
            HasSelectSpeciallSkill = false;
        }
    }
    /// <summary>
    /// �}���K����₩�ǂ��� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    public void SetIsMulliganCard()
    {
        this.isMulliganCard = true;
    }
    /// <summary>
    /// �}���K�����邩�ǂ��� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isMulligan"></param>
    public void SetIsMulligan(bool isMulligan)
    {
        this.isMulligan = isMulligan;
    }
    /// <summary>
    /// ���j�b�g���_���[�W���󂯂����̏��� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
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
    /// ���j�b�g���񕜂��󂯂����̏��� ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="hl"></param>
    public void Heal(int hl)
    {
        hp += hl;
        if (hp > maxHP)
        {
            hp = maxHP;
        }
    }
    /// <summary>
    /// ���j�b�g���q�[���[�ɍU������ ��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
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
    /// �U���\�ɂ���@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    public void SetCanAttack(bool canAttack)
    {
        this.canAttack = canAttack;
    }
    /// <summary>
    /// �t�B�[���h�ɒu����Ă��邩�ǂ�����ݒ�@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isFieldCard"></param>
    public void SetIsFieldCard(bool isFieldCard)
    {
        this.isFieldCard = isFieldCard;
    }
    /// <summary>
    ///�@�ǂ̃t�B�[���h�̃��j�b�g����ݒ�@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="thisFieldID"></param>
    public void SetThisFieldID(int thisFieldID)
    {
        this.thisFieldID = thisFieldID;
    }
    public void SetIsNotSummonThisTurn()
    {
        this.isSummonThisTurn = false;
    }
    /// <summary>
    /// ���������ǂ�����ݒ�@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isTaunt"></param>
    public void SetIsTaunt(bool isTaunt)
    {
        this.isTaunt = isTaunt;
    }
    /// <summary>
    /// �A�������L�����ǂ�����ݒ�@��{�I��CardController��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="isActiveDoubleAction"></param>
    public void SetIsActiveDoubleAction(bool isActiveDoubleAction)
    {
        this.isActiveDoubleAction = isActiveDoubleAction;
    }
    public void SetIsSeal(bool isSeal)
    {
        this.isSeal = isSeal;
        SetDefaultStats();
        isTaunt = false;
    }
    public void Buff(int atk, int hp)
    {
        this.atk += atk;
        maxHP += hp;
        this.hp += hp;
    }
    public void DeBuff(int atk, int hp)
    {
        this.atk -= atk;
        maxHP -= hp;
        this.hp -= hp;
        if (atk < 0)
        {
            atk = 0;
        }
        if (hp < 0)
        {
            hp = 0;
        }
    }
    /// <summary>
    /// �K��l�ɖ߂��@��ɕ���p
    /// </summary>
    public void SetDefaultStats()
    {
        if (hp > defaultHP)
        {
            maxHP = hp = defaultHP;
        }
        else
        {
            maxHP = defaultHP;
        }
        atk = defaultATK;
    }
}
