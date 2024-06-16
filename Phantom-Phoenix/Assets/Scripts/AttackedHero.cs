using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//UΞΫ€Μq[[
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController target;
    public void OnDrop(PointerEventData eventData)
    {
        target = this.gameObject.GetComponent<HeroController>();

        //U€ͺUΒ\ΕAplayerΖenemyΜΦWΘη
        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayer)
        {

            if (SkillManager.instance.CheckCanAttackHero(attacker, target)){
                //U·ι€ΪόΜ
                GameManager.instance.AttackTohero(attacker, attacker.model.isPlayerCard);
            }

        }
    }
}
