using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//�U���Ώۑ��̃q�[���[
public class AttackedHero : MonoBehaviour, IDropHandler
{
    private HeroController heroController;
    public void OnDrop(PointerEventData eventData)
    {
        heroController = this.gameObject.GetComponent<HeroController>();

        //�U�������U���\�ŁAplayer��enemy�̊֌W�Ȃ�
        if (eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != heroController.model.isPlayer)
        {

            //�E�H�[���⒧��������Ă���Ȃ� ���S�������\���Ŕ��肷�� �X�}�[�g�ȕ��@���v�����Ȃ������c
            //fieldID�́A�@
            //             ���O��    �O����
            //              4   1   |   7   10
            //playerHero    5   2   |   8   11  enemyHero
            //              6   3   |   9   12
            //�ƂȂ��Ă���

            if (heroController.model.isPlayer) //hero��player�Ȃ�A�m�F����field��player��
            {
                if (GameManager.instance.isAny����(true)) { return; }

                //�E�H�[��
                if (GameManager.instance.isWall(true)) { return; }
            }
            else //����ȊO��enemy��
            {
                if (GameManager.instance.isAny����(false)) { return; }

                //�E�H�[��
                if (GameManager.instance.isWall(false)) { return; }
            }

            //�U�����鑤�ڐ��̏���
            GameManager.instance.AttackTohero(attacker, attacker.model.isPlayerCard);

        }
    }
}
