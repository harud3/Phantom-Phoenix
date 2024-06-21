using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//�U���Ώۑ��̃q�[���[
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController target;
    public void OnDrop(PointerEventData eventData)
    {
        target = this.gameObject.GetComponent<HeroController>();

        //�U�������U���\�ŁAplayer��enemy�̊֌W�Ȃ�
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
                //�U�����鑤�ڐ��̏���
                GameManager.instance.AttackTohero(attacker);
            }

        }
    }
}
