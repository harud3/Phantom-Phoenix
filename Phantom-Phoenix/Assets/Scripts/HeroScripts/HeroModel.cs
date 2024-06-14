using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroModel
{
    public string name;
    public int hp;
    public int atk;
    public int maxMP = 0;
    public int mp;
    public Sprite icon;
    [NonSerialized]
    public bool isAlive;
    [NonSerialized]
    public bool canAttack;
    [NonSerialized]
    public bool isWall;
    [NonSerialized]
    public bool usedTensionCard;
    int Tension;
    [SerializeField]
    public readonly bool isPlayer;

    public HeroModel(int heroID, bool isPlayer)
    {
        HeroEntity heroEntity = Resources.Load<HeroEntity>($"HeroEntityList/Hero{heroID}");
        name = heroEntity.name;
        hp = heroEntity.hp;
        atk = 0;
        mp =  maxMP = 0;
        icon = heroEntity.icon;
        isAlive = true;
        canAttack = false;
        isWall = false;
        usedTensionCard = false;
        this.isPlayer = isPlayer;
        Tension = 0;
    }
    public void Damage(int dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            hp = 0;
            isAlive = false;
        }
    }
    public void UpMaxMP()
    {
        if(maxMP < 10)
        {
            maxMP += 1;
        }
    }
    public void ResetMP()
    {
        UpMaxMP();
        mp = maxMP;
    }
    public void HealMP(int heal)
    {
        mp += heal;
        if(mp > maxMP)
        {
            mp = maxMP;
        }
    }
    public void ReduceMP(int reduce)
    {
        mp -= reduce;
        if (mp < 0)
        {
            mp = 0;
        }
    }
}
