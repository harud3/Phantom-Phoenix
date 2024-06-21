using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//UŒ‚‘ÎÛ‘¤‚Ìƒq[ƒ[
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController target;
    public void OnDrop(PointerEventData eventData)
    {
        target = this.gameObject.GetComponent<HeroController>();

        //UŒ‚‘¤‚ªUŒ‚‰Â”\‚ÅAplayer‚Æenemy‚ÌŠÖŒW‚È‚ç
        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayer)
        {

            if (SkillManager.instance.CheckCanAttackHero(attacker, target)){
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendAttackToHero(attacker.model.fieldID);
                }
                AudioManager.instance.SoundCardAttack();
                //UŒ‚‚·‚é‘¤–Úü‚Ìˆ—
                GameManager.instance.AttackTohero(attacker);
            }

        }
    }
}
