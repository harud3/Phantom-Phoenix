using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//çUåÇëŒè€ë§
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroModel heroModel;
    public void OnDrop(PointerEventData eventData)
    {
        heroModel = this.gameObject.GetComponent<HeroController>().model;

        if (heroModel.isWall) { return; }

        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack && attacker.model.isPlayerCard != this.GetComponent<HeroController>().GetIsPlayer())
        {
            GameManager.instance.AttackTohero(attacker, this.tag == "PlayerHero" ? false : true);
        }
    }
}
