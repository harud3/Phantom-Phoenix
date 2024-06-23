using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//�U���Ώۑ��̃J�[�h �J�[�h�ɂ��Ă���
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
            if (FieldManager.instance.CheckCanAttackUnit(attacker, target))
            {
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendCardBattle(attacker.model.thisFieldID, target.model.thisFieldID); //�o�g������ΐ푊��ɑ��M
                }
                ExecuteCardsBattle(attacker, target);
                
            }

            
        }
    }
    void  ExecuteCardsBattle(CardController attacker, CardController target)
    {
        AudioManager.instance.SoundCardAttack();
        //�J��̋V
        StartCoroutine(GameManager.instance.CardsBattle(attacker, target));
    }
}
