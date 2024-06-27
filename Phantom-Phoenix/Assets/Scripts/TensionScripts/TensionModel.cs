using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TensionModel
{
    public int tension {  get; private set; }
    public bool isTensionUsedThisTurn { get; private set; }
    public int tensionSpellUsedCnt { get; private set; }
    public bool isPlayer { get; private set; }
    public TensionModel(bool isPlayer)
    {
        tension = 0;
        isTensionUsedThisTurn = false;
        tensionSpellUsedCnt = 0;
        this.isPlayer = isPlayer;
    }
    public void SetTension(int tension)
    {
        this.tension = tension;
    }
    public void UseTensionCard()
    {
        if (tension < 3)
        {
            tension++;
            isTensionUsedThisTurn = true;
            GameManager.instance.ReduceMP(1, isPlayer);
        }
    }
    public void CanUseTensionCard(bool canUseTensionCard)
    {
        isTensionUsedThisTurn = !canUseTensionCard;
    }
    public void PlusTensionSpellUsedCnt()
    {
        tensionSpellUsedCnt++;
    }
}
