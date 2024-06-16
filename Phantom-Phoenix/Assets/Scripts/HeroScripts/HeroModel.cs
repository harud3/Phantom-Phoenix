using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �q�[���[�̎���
/// </summary>
public class HeroModel
{
    public string name {  get; private set; }
    public int hp {  get; private set; }
    public int atk {  get; private set; }
    public int maxMP {  get; private set; }
    public int mp {  get; private set; }
    public Sprite icon {  get; private set; }
    public bool isPlayer { get; private set; }
    public bool isAlive {  get; private set; }
    public bool canAttack {  get; private set; }
    public bool isWall {  get; private set; }
    public bool usedTensionCard {  get; private set; }
    public int Tension { get; private set; }
    

    public HeroModel(int heroID, bool isPlayer)
    {
        //heroID����ɑΏۂ̃q�[���[�f�[�^���擾����
        HeroEntity heroEntity = Resources.Load<HeroEntity>($"HeroEntityList/Hero{heroID}");
        name = heroEntity.name;
        hp = heroEntity.hp;
        atk = 0;
        mp =  maxMP = 0;
        icon = heroEntity.icon;
        this.isPlayer = isPlayer;
        isAlive = true;
        canAttack = false;
        isWall = false;
        usedTensionCard = false;
        Tension = 0;
    }
    /// <summary>
    /// �q�[���[���_���[�W���󂯂����̏��� view.Reshow()���K�v�Ȃ̂ŁAController����ĂԂ��� ���ڌĂ΂Ȃ�
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
    /// �^�[���J�n����MP���Z�b�g view.Reshow()���K�v�Ȃ̂ŁAController����ĂԂ��� ���ڌĂ΂Ȃ�
    /// </summary>
    public void ResetMP()
    {
        UpMaxMP();
        mp = maxMP;
    }
    /// <summary>
    /// (��Ƀ^�[���J�n����)�ő�MP����{ view.Reshow()���K�v�Ȃ̂ŁAController����ĂԂ��� ���ڌĂ΂Ȃ�
    /// </summary>
    public void UpMaxMP()
    {
        if(maxMP < 10)
        {
            maxMP += 1;
        }
    }
    /// <summary>
    /// (��ɃJ�[�h���o��������)MP�����炷 view.Reshow()���K�v�Ȃ̂ŁAController����ĂԂ��� ���ڌĂ΂Ȃ�
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
    /// ���炩�̊O���v���ɂ��AMP���񕜂��鎞 view.Reshow()���K�v�Ȃ̂ŁAController����ĂԂ��� ���ڌĂ΂Ȃ�
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
}