using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//UŒ‚‘ÎÛ‘¤‚ÌƒJ[ƒh
public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //UŒ‚‘¤‚ªUŒ‚‰Â”\‚ÅAplayer‚Æenemy‚ÌŠÖŒW‚È‚ç
        if(eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && GetComponent<CardController>() is var target
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayerCard)
        {
            if (SkillManager.instance.CheckCanAttackUnit(attacker, target))
            {
                //ŠJí‚Ì‹V
                GameManager.instance.CardsBattle(attacker, target);
                SkillManager.instance.ExecutePierce(attacker, target);
            }

            
        }
    }
}
