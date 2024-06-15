using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//攻撃対象側のカード
public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //攻撃側が攻撃可能で、playerとenemyの関係なら
        if(eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && GetComponent<CardController>() is var target
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayerCard)
        {

            //ブロックや挑発がされているならをゴリ押し構文で判定する スマートな方法を思いつけなかった…
            //このスクリプトがアタッチされているカードの親(つまり置かれているfield)のfieldIDを取得する
            //fieldIDは、　
            //             後列前列    前列後列
            //              4   1   |   7   10
            //playerHero    5   2   |   8   11  enemyHero
            //              6   3   |   9   12
            //となっている
            var thisFieldID = this.transform.parent.GetComponent<DropField>().fieldID;

            //挑発
            if (!target.model.is挑発) //is挑発はfield1,2,3またはfield7,8,9にいる時にtrueとなる　よって、targetがis挑発してるなら即開戦でOK　
            {
                if (target.model.isPlayerCard) //targetがplayerCardなら、攻撃対象側のfieldもplayer側
                {
                    if (GameManager.instance.isAny挑発(true)) { return; }
                    
                    if(GameManager.instance.isBlock(true, thisFieldID)) {  return; }
                }
                else //それ以外のfieldはenemy側
                {
                    if (GameManager.instance.isAny挑発(false)) { return; }

                    if (GameManager.instance.isBlock(false, thisFieldID)) { return; }

                }
            }

            //開戦の儀
            GameManager.instance.CardsBattle(attacker, target);
        }
    }
}
