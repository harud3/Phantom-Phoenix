using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

/// <summary>
/// �f�b�L�Ґ���ʁ@�J�[�h���f�b�L���Ƀh���b�v�������̏���
/// </summary>
public class EditDeckDropDeck : MonoBehaviour, IDropHandler
{
    [SerializeField]
    TextMeshProUGUI hintText;
    public void OnDrop(PointerEventData eventData)
    {

        EditDeckCardController cc = eventData.pointerDrag.GetComponent<EditDeckCardController>();
        if (cc == null) { return; }

        if (this.transform.childCount < 30)
        {
            var rarity = cc.model.rarity;
            var cardID = cc.model.cardID;
            int maxCount = rarity == CardEntity.Rarity.SSR ? 1 : 3 ; //���A�x�ŏ���������ς��
            var cardcount = 1;
#if UNITY_EDITOR
            maxCount = 30; //�e�X�g�p
#endif
            foreach (Transform t in this.transform)
            {
                if (t.gameObject.GetComponent<EditDeckCardController>().model.cardID == cardID) { cardcount++; }

                if (cardcount > maxCount) //�~���肾�悻���
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeText1());
                    return;
                }
            }

            //�J�[�h���f�b�L�̎q�ɂ���
            cc.movement.SetDefaultParent(this.transform);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ChangeText2());
            return;
        }
    }
    IEnumerator ChangeText1()
    {
        hintText.text = "R/SR��3���@SSR ��1�� �܂�";
        yield return new WaitForSeconds(2f);
        hintText.text = "�J�[�h���f�b�L�Ƀh���b�O&�h���b�v";
        StopAllCoroutines();
    }
    IEnumerator ChangeText2()
    {
        hintText.text = "�f�b�L��30���ł�";
        yield return new WaitForSeconds(2f);
        hintText.text = "�J�[�h���f�b�L�Ƀh���b�O&�h���b�v";
        StopAllCoroutines();
    }
}
