using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//�U���Ώۑ��̃q�[���[ �q�[���[�ɂ��Ă���
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController target;
    public void OnDrop(PointerEventData eventData)
    {
        target = this.gameObject.GetComponent<HeroController>();

        //�U�������U���\�ŁAplayer��enemy�̊֌W�Ȃ�
        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayer
            && GameManager.instance.isPlayerTurn)
        {

            if (FieldManager.instance.CheckCanAttackHero(attacker, target)){ //�E�H�[���⒧��������ƍU���s��
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendAttackToHero(attacker.model.thisFieldID); //�q�[���[���U�����邱�Ƃ�ΐ푊��ɑ��M
                }
                AudioManager.instance.SoundCardAttack();
                //�U�����鑤�ڐ��̏���
                GameManager.instance.AttackTohero(attacker);
            }

        }
    }
}
