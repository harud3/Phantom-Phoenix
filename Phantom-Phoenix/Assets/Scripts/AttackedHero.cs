using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//攻撃対象側のヒーロー
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController heroController;
    public void OnDrop(PointerEventData eventData)
    {
        heroController = this.gameObject.GetComponent<HeroController>();

        //攻撃側が攻撃可能で、playerとenemyの関係なら
        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != heroController.model.isPlayer)
        {

            //ウォールや挑発がされているなら をゴリ押し構文で判定する スマートな方法を思いつけなかった…
            //fieldIDは、　
            //             後列前列    前列後列
            //              4   1   |   7   10
            //playerHero    5   2   |   8   11  enemyHero
            //              6   3   |   9   12
            //となっている

            if (heroController.model.isPlayer) //heroがplayerなら、確認するfieldもplayer側
            {
                if (GameManager.instance.isAny挑発(true)) { return; }

                //ウォール
                if (GameManager.instance.isWall(true)) { return; }
            }
            else //それ以外はenemy側
            {
                if (GameManager.instance.isAny挑発(false)) { return; }

                //ウォール
                if (GameManager.instance.isWall(false)) { return; }
            }

            //攻撃する側目線の処理
            GameManager.instance.AttackTohero(attacker, attacker.model.isPlayerCard);

        }
    }
}
