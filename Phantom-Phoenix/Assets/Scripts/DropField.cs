using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �J�[�h��field�Ƀh���b�v�������̏���
/// </summary>
public class DropField : MonoBehaviour, IDropHandler
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
        CardController cardController = eventData.pointerDrag.GetComponent<CardController>();
        if (cardController == null�@|| cardController.model.category == CardEntity.Category.spell 
            || isPlayerField != cardController.model.isPlayerCard || cardController.model.isFieldCard)
        {
            return;
        }

        if (cardController.movement.isDraggable && this.transform.childCount == 0)
        {
            //�J�[�h��field�ɒu������
            cardController.movement.SetDefaultParent(this.transform);
            //model��fieldID��ݒ肵�Afield�ɒu�����̏������s��
            cardController.MoveField(fieldID);
            cardController.putOnField(isPlayerField);
            
        }
        
    }
}
