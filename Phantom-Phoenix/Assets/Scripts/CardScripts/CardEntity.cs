using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // アトリビュートを付与
public class CardEntity
{
    public int ID;
    public Hero hero;
    public Category category;
    public Rarity rarity;
    public string name;
    public int cost;
    public int atk;
    public int hp;

    public Skill skill1, skill2, skill3;
    public Target target;
    public string text;

    public enum Hero { common, elf, witch, king, knight, devil }
    public enum Category { unit, spell, item }
    public enum Rarity { R, SR, SSR }

    public enum Skill { none, fast, taunt, snipe, pierce, doubleAction } //なし、即撃、挑発、狙撃、貫通、連撃　これらは効果持ちカードが多くなりがちな効果群なので特別枠
    /// <summary>
    /// 効果範囲　ユニットの召喚時選択効果もここで管理する、スペルの効果範囲
    /// </summary>
    public enum Target
    {
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
