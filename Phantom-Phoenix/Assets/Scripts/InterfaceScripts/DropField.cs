using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// �J�[�h��field�Ƀh���b�v�������̏���
/// </summary>
public class DropField : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField]
    private bool isPlayerField; //player��enemy���@���O�ɃC���X�y�N�^�[��Őݒ�
    [SerializeField]
    private int _fieldID; //fieldID�������Ă���
    public int fieldID { get { return _fieldID; } }
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; }//TODO

        //field�ɒu��������́A
        //cardConroller������A
        //unit�ŁA
        //player�̂��̓��m��enemy�̂��̓��m�ł���A
        //��D�̃J�[�h�ł���A
        //�h���b�O�\�ł���A
        //field�ɑ��̃J�[�h���u����Ă��Ȃ����@�ł���
        CardController cc = eventData.pointerDrag.GetComponent<CardController>();
        if (cc == null�@|| cc.model.category == CardEntity.Category.spell 
            || isPlayerField != cc.model.isPlayerCard || cc.model.isFieldCard)
        {
            return;
        }

        if (cc.movement.isDraggable && this.transform.childCount == 0)
        {
            //�J�[�h��field�ɒu������
            cc.movement.SetDefaultParent(this.transform, fieldID);
            //model��fieldID��ݒ肵�Afield�ɒu�����̏������s��
            cc.MoveField(fieldID);
            cc.putOnField(isPlayerField);
        }
        
    }
}
