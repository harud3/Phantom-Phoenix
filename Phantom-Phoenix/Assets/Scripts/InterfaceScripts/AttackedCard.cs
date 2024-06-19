using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//�U���Ώۑ��̃J�[�h
public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //�U�������U���\�ŁAplayer��enemy�̊֌W�Ȃ�
        if(eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && GetComponent<CardController>() is var target
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayerCard)
        {
            if (SkillManager.instance.CheckCanAttackUnit(attacker, target))
            {
                //�J��̋V
                GameManager.instance.CardsBattle(attacker, target);
                SkillManager.instance.ExecutePierce(attacker, target);
            }

            
        }
    }
}
