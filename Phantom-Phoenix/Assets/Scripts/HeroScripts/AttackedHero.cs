using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//攻撃対象側のヒーロー ヒーローについている
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController target;
    public void OnDrop(PointerEventData eventData)
    {
        target = this.gameObject.GetComponent<HeroController>();

        //攻撃側が攻撃可能で、playerとenemyの関係なら
        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayer
            && GameManager.instance.isPlayerTurn)
        {

            if (FieldManager.instance.CheckCanAttackHero(attacker, target)){ //ウォールや挑発があると攻撃不可
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendAttackToHero(attacker.model.thisFieldID); //ヒーローを攻撃することを対戦相手に送信
                }
                AudioManager.instance.SoundCardAttack();
                //攻撃する側目線の処理
                GameManager.instance.AttackTohero(attacker);
            }

        }
    }
}
