using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �X�y���̔����O���Ǘ�����@Card�v���n�u��Hero�u���n�u�ƃt�B�[���h�ɂ��Ă�
/// </summary>
public class DropSpellField : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; } //�����̃^�[���ł͂Ȃ��̂ɒu�����Ƃ��Ȃ���

        var targetC = GetComponent<CardController>() ?? null; //����Ƃ�����
        var targetH = GetComponent<HeroController>() ?? null; //����Ƃ�����

        if(targetC != null && !targetC.model.isFieldCard) { return; }�@//��D�ɂ���J�[�h��Ώۂɂ��Ă͂����Ȃ�
        if (eventData.pointerDrag.GetComponent<CardController>() is var spell)
        {
            //MP���߂̃X�y����X�y���ȊO�͒ʂ��Ȃ�
            if (spell == null || spell.model.cost > GameManager.instance.GetHeroMP(spell.model.isPlayerCard) || spell.model.category != CardEntity.Category.spell) { return; }

            var handIndex = spell.GetComponent<CardMovement>().siblingIndex; //�ΐ푊��Ɏ�D�̂ǂ̃J�[�h���g�����𑗐M����̂�

            //�^�[�Q�b�g���𑗂�@ExecuteSpellContents�̕��ŁA�Ώۋ敪����ɕ��ނ��s�����s�ۂ����߂�  ���̂��߁A �Ƃ肠�����Ώۂ𑗂��Ă����΂���
            if (targetC != null)
            {
                //�Ƃ肠�����ACardController��n���Ă���
                if (spell.ExecuteSpellContents(targetC) && GameDataManager.instance.isOnlineBattle)
                {
                    SendExecuteSpellContents(handIndex, targetC.model.thisFieldID);
                }
            }
            else if (targetH != null)
            {
                //�Ƃ肠�����AHeroController��n���Ă���
                if (spell.ExecuteSpellContents(targetH) && GameDataManager.instance.isOnlineBattle)
                {
                    SendExecuteSpellContents(handIndex, targetH.model.isPlayer ? 13 : 14);
                }
            }
            else
            {
                //�ǂ����悤���Ȃ��̂�null��n���Ă���
                if (spell.ExecuteSpellContents<Controller>(null) && GameDataManager.instance.isOnlineBattle)
                {
                    SendExecuteSpellContents(handIndex);
                }
            }

        }
    }

    private void SendExecuteSpellContents(int handIndex, int targetID = 0)
    {
        //targetID�͖����ڐ��Ȃ̂ŁA�G�ڐ��ɕϊ����Ă���
        if(1 <= targetID && targetID <= 12) { targetID = FieldManager.instance.ChangeFieldID(targetID); }
        targetID = targetID == 13 ? 14 : targetID == 14 ? 13 : targetID; //13��14 14��13 ���͂��̂܂�
        GameManager.instance.SendExecuteSpellContents(handIndex, targetID);
    }
}
