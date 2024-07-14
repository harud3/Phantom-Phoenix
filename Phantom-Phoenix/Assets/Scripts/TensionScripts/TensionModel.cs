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
    /// �e���V�����̒l��ݒ肷��
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
    /// �e���V�����J�[�h���g��
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
    /// ���̃^�[���Ƀe���V�����J�[�h���g�������ǂ�����ݒ肷�� == �g�p�ς݂Ȃ炱�̃^�[�����̓e���V�����J�[�h���g�p�ł��Ȃ�
    /// </summary>
    public void CanUseTensionCard(bool canUseTensionCard)
    {
        isTensionUsedThisTurn = !canUseTensionCard;
    }
    /// <summary>
    /// �e���V�����X�y���g�p��++
    /// </summary>
    public void PlusTensionSpellUsedCnt()
    {
        tensionSpellUsedCnt++;
    }
}
