using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ヒーローの実体
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
        //heroIDを基に対象のヒーローデータを取得する
        HeroEntity heroEntity = Resources.Load<HeroEntity>($"HeroEntityList/Hero{heroID}");
        name = heroEntity.name;
        hp = heroEntity.hp;
        atk = 0;
        mp =  maxMP = 0;
#if UNITY_EDITOR
        maxMP = 9;
#endif
        icon = heroEntity.icon;
        this.isPlayer = isPlayer;
        isAlive = true;
        canAttack = false;
        isWall = false;
        usedTensionCard = false;
        Tension = 0;
    }
    /// <summary>
    /// ヒーローがダメージを受けた時の処理 view.Reshow()も必要なので、Controllerから呼ぶこと 直接呼ばない
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
    /// ターン開始時のMPリセット view.Reshow()も必要なので、Controllerから呼ぶこと 直接呼ばない
    /// </summary>
    public void ResetMP()
    {
        ChangeMaxMP(1);
        mp = maxMP;
    }
    /// <summary>
    /// (主にターン開始時に)最大MP上限＋ view.Reshow()も必要なので、Controllerから呼ぶこと 直接呼ばない
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
    /// (主にカードを出した時に)MPを減らす view.Reshow()も必要なので、Controllerから呼ぶこと 直接呼ばない
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
    /// 何らかの外部要因により、MPが回復する時 view.Reshow()も必要なので、Controllerから呼ぶこと 直接呼ばない
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
