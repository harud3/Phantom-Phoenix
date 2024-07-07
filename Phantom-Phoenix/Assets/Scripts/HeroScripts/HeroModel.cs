using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �q�[���[�̎���
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
        //heroID����ɑΏۂ̃q�[���[�f�[�^���擾����
        HeroEntity heroEntity = Resources.Load<HeroEntity>($"HeroEntityList/Hero{heroID}");
        this.heroID = heroID;
        character = heroEntity.character;
        name = heroEntity.name;
        atk = 0;
        maxHP = hp = heroEntity.hp;
        mp =  maxMP = 5; //TODO �����l0
        this.isPlayer = isPlayer;
        isAlive = true;

        plusSpellDamage = 0;
        plusSpellCost = 0;
        minusSpellCost = 0;
    }
    /// <summary>
    /// �q�[���[���_���[�W���󂯂����̏��� ���ڌĂ΂Ȃ�
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
    /// �q�[���[���񕜂��󂯂����̏��� ���ڌĂ΂Ȃ�
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
    /// �^�[���J�n����MP���Z�b�g ���ڌĂ΂Ȃ�
    /// </summary>
    public void ResetMP()
    {
        ChangeMaxMP(1);
        mp = maxMP;
    }
    /// <summary>
    /// (��Ƀ^�[���J�n����)�ő�MP����{ ���ڌĂ΂Ȃ�
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
    /// (��ɃJ�[�h���o��������)MP�����炷 ���ڌĂ΂Ȃ�
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
    /// ���炩�̊O���v���ɂ��AMP���񕜂��鎞 ���ڌĂ΂Ȃ�
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
