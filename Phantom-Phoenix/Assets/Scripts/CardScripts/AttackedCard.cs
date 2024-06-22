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
                StartCoroutine(ExecuteCardsBattle(attacker, target));
                
            }

            
        }
    }
    IEnumerator  ExecuteCardsBattle(CardController attacker, CardController target)
    {
        yield return new WaitForSeconds(0.25f);
        //�J��̋V
        AudioManager.instance.SoundCardAttack();
        GameManager.instance.CardsBattle(attacker, target);
        SkillManager.instance.ExecutePierce(attacker, target);
    }
}
