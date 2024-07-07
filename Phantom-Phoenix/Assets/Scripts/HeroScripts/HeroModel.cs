using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ヒーローの実体
/// </summary>
public class HeroModel
{
    public int heroID {  get; private set; }
    public Sprite character { get; private set; }
    public string name {  get; private set; }
    public int atk { get; private set; }
    public int maxHP { get; private set; }
    public int hp {  get; private set; }
    public int maxMP {  get; private set; }
    public int mp {  get; private set; }
    
    public bool isPlayer { get; private set; }
    public bool isAlive {  get; private set; }
    public int plusSpellDamage { get; private set; }
    public int plusSpellCost { get; private set; }
    public int minusSpellCost {  get; private set; }

    public HeroModel(int heroID, bool isPlayer)
    {
        //heroIDを基に対象のヒーローデータを取得する
        HeroEntity heroEntity = Resources.Load<HeroEntity>($"HeroEntityList/Hero{heroID}");
        this.heroID = heroID;
        character = heroEntity.character;
        name = heroEntity.name;
        atk = 0;
        maxHP = hp = heroEntity.hp;
        mp =  maxMP = 5; //TODO 初期値0
        this.isPlayer = isPlayer;
        isAlive = true;

        plusSpellDamage = 0;
        plusSpellCost = 0;
        minusSpellCost = 0;
    }
    /// <summary>
    /// ヒーローがダメージを受けた時の処理 直接呼ばない
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
    /// ヒーローが回復を受けた時の処理 直接呼ばない
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
    public void Concede()
    {
        isAlive = false;
    }
    /// <summary>
    /// ターン開始時のMPリセット 直接呼ばない
    /// </summary>
    public void ResetMP()
    {
        ChangeMaxMP(1);
        mp = maxMP;
    }
    /// <summary>
    /// (主にターン開始時に)最大MP上限＋ 直接呼ばない
    /// </summary>
    public void ChangeMaxMP(int up)
    {
        maxMP += up;
        if (maxMP > 10)
        {
            maxMP = 10;
        }
        else if(maxMP < 0){ maxMP = 0; }
    }
    /// <summary>
    /// (主にカードを出した時に)MPを減らす 直接呼ばない
    /// </summary>
    /// <param name="reduce"></param>
    public void ReduceMP(int reduce)
    {
        mp -= reduce;
        if (mp < 0)
        {
            mp = 0;
        }
    }
    /// <summary>
    /// 何らかの外部要因により、MPが回復する時 直接呼ばない
    /// </summary>
    /// <param name="heal"></param>
    public void HealMP(int heal)
    {
        mp += heal;
        if(mp > maxMP)
        {
            mp = maxMP;
        }
    }
    public void spellDamageBuff(int buff)
    {
        plusSpellDamage += buff;
        if (plusSpellDamage < 0)
        {
            plusSpellDamage = 0;
        }

    }
    public void SetMinusSpellCost(int minus)
    {
        minusSpellCost = minus;
        if (plusSpellDamage < 0)
        {
            plusSpellDamage = 0;
        }

    }

}
