using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードデータ
/// </summary>
[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]
public class CardEntity : ScriptableObject
{
    public Sprite icon;
    public new string name;
    public int cost;
    public int atk;
    public int hp;
    public Category category;
    public Rarity rarity;
    public Skill skill1, skill2, skill3;
    public string cardText;
    public SpellTarget spellTarget;

    public enum Category { unit, spell, item }
    public enum Rarity { R, SR, SSR}

    public enum Skill { none, fast, taunt, snipe, pierce, doubleAction}
    /// <summary>
    /// そのスペルの効果範囲　このカードがユニットならnone
    /// </summary>
    public enum SpellTarget { 
        none, //対象なし
        unit, //ユニット単体
        enemyUnit, //敵ユニット単体
        playerUnit, //味方ユニット単体
        hero, //ヒーロー単体
        unitOrHero, //単体
        enemy, //敵単体
        player, //味方単体

        area, //範囲
        //enemyUnits, //敵ユニット全体
        //playerUnits, //味方ユニット全体
        //enemys, //敵全体
        //players, //味方全体
        //units, //ユニット全体
        //enemyhero, //敵ヒーロー単体　1体しか存在せず選ぶ必要がないため
        //playerhero, //味方ヒーロー単体 1体しか存在せず選ぶ必要がないため
        //heros, //両ヒーロー
        //all, //敵味方全体

        selectionArea, //選択範囲
        selectionPlayerArea, //敵選択範囲
        selectionEnemyArea, //味方選択範囲
        //UnitsV, //ユニット縦一列
        //UnitsH, //ユニット横一列
        //enemyUnitsV, //敵ユニット縦一列 
        //enemyUnitsH, //敵ユニット横一列
        //playerUnitsV, //味方ユニット縦一列
        //playerUnitsH, //味方ユニット横一列
    }
}
