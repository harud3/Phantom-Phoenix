using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

            //�u���b�N�⒧��������Ă���Ȃ���S�������\���Ŕ��肷�� �X�}�[�g�ȕ��@���v�����Ȃ������c
            //���̃X�N���v�g���A�^�b�`����Ă���J�[�h�̐e(�܂�u����Ă���field)��fieldID���擾����
            //fieldID�́A�@
            //             ���O��    �O����
            //              4   1   |   7   10
            //playerHero    5   2   |   8   11  enemyHero
            //              6   3   |   9   12
            //�ƂȂ��Ă���
            var thisFieldID = this.transform.parent.GetComponent<DropField>().fieldID;

            //����
            if (!target.model.is����) //is������field1,2,3�܂���field7,8,9�ɂ��鎞��true�ƂȂ�@����āAtarget��is�������Ă�Ȃ瑦�J���OK�@
            {
                if (target.model.isPlayerCard) //target��playerCard�Ȃ�A�U���Ώۑ���field��player��
                {
                    if (GameManager.instance.isAny����(true)) { return; }
                    
                    if(GameManager.instance.isBlock(true, thisFieldID)) {  return; }
                }
                else //����ȊO��field��enemy��
                {
                    if (GameManager.instance.isAny����(false)) { return; }

                    if (GameManager.instance.isBlock(false, thisFieldID)) { return; }

                }
            }

            //�J��̋V
            GameManager.instance.CardsBattle(attacker, target);
        }
    }
}
