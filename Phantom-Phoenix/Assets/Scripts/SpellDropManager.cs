using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// �X�y���̔����O���Ǘ�����@Card�v���n�u��Hero�u���n�u�ƃt�B�[���h�ɂ��Ă�
/// </summary>
public class SpellDropManager : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var targetC = GetComponent<CardController>() ?? null; //����Ƃ�����
        var targetH = GetComponent<HeroController>() ?? null; //����Ƃ�����

        if(targetC != null && !targetC.model.isFieldCard) { return; }�@//��D�ɂ���J�[�h��Ώۂɂ��Ă͂����Ȃ�
        if (eventData.pointerDrag.GetComponent<CardController>() is var spell)
        {
            
            if (spell == null || spell.model.category != CardEntity.Category.spell) { return; } //�X�y���ȊO�͒ʂ��Ȃ�
            if (targetC != null)
            {
                spell.ExecuteSpellContents(targetC); //�Ƃ肠�����ACardController��n���Ă���
            }
            else if (targetH != null)
            {
                spell.ExecuteSpellContents(targetH); //�Ƃ肠�����AHeroController��n���Ă���
            }
            else
            {
                spell.ExecuteSpellContents<Controller>(null); //�ǂ����悤���Ȃ��̂�null��n���Ă���
            }

        }
    }
}
