using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TensionModel
{
    public int tension {  get; private set; }
    public int tensionID { get; private set; }
    public bool isTensionUsedThisTurn { get; private set; }
    public int tensionSpellUsedCnt { get; private set; }
    public bool isPlayer { get; private set; }
    public TensionModel(bool isPlayer, int useHeroID)
    {
        tension = 0;
        tensionID = useHeroID;
        isTensionUsedThisTurn = false;
        tensionSpellUsedCnt = 0;
        this.isPlayer = isPlayer;
    }
    /// <summary>
    /// テンションの値を設定する
    /// </summary>
    public void SetTension(int tension)
    {
        if(this.tension < tension)
        {
            SkillManager.instance.SkillCausedByTension((isPlayer ? Enumerable.Range(1, 6) : Enumerable.Range(7, 6)).ToArray());
        }
        this.tension = tension;
    }
    /// <summary>
    /// テンションカードを使う
    /// </summary>
    public void UseTensionCard()
    {
        if (tension < 3)
        {
            tension++;
            isTensionUsedThisTurn = true;
            GameManager.instance.ReduceMP(1, isPlayer);
            SkillManager.instance.SkillCausedByTension((isPlayer ? Enumerable.Range(1, 6) : Enumerable.Range(7, 6)).ToArray());
        }
    }
    /// <summary>
    /// このターンにテンションカードを使ったかどうかを設定する == 使用済みならこのターン中はテンションカードを使用できない
    /// </summary>
    public void CanUseTensionCard(bool canUseTensionCard)
    {
        isTensionUsedThisTurn = !canUseTensionCard;
    }
    /// <summary>
    /// テンションスペル使用回数++
    /// </summary>
    public void PlusTensionSpellUsedCnt()
    {
        tensionSpellUsedCnt++;
    }
}
