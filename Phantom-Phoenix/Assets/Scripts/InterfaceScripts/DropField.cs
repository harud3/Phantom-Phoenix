using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor.Experimental.GraphView;
using System.Linq;

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
            if (cc.model.HasSelectSpeciallSkill && IsExistTarget())
            {
                StartCoroutine(waitPlayerClick(cc));
            }
            else
            {
                //�J�[�h��field�ɒu������
                cc.movement.SetDefaultParent(this.transform, fieldID);
                //model��fieldID��ݒ肵�Afield�ɒu�����̏������s��
                cc.MoveField(fieldID);
                cc.putOnField(isPlayerField);
            }
            
        }
        
    }
    IEnumerator waitPlayerClick(CardController cc)
    {
        cc.gameObject.SetActive(false);
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        cc.gameObject.SetActive(true);
        GameObject clickedGameObject = null;

        //RaycastAll�̈����iPointerEventData�j�쐬
        PointerEventData pointData = new PointerEventData(EventSystem.current);

        //RaycastAll�̌��ʊi�[�pList
        List<RaycastResult> RayResult = new List<RaycastResult>();

        //PointerEventData�Ƀ}�E�X�̈ʒu���Z�b�g
        pointData.position = Input.mousePosition;
        //RayCast�i�X�N���[�����W�j
        EventSystem.current.RaycastAll(pointData, RayResult);
        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card").FirstOrDefault().gameObject;
        if (targetCheck(cc, clickedGameObject) is var x && x.passed)
        {
            //�J�[�h��field�ɒu������
            cc.movement.SetDefaultParent(this.transform, fieldID, x.targets.Select(i => i.model.fieldID).ToArray());
            cc.transform.SetParent(this.transform);
            //model��fieldID��ݒ肵�Afield�ɒu�����̏������s��
            cc.MoveField(fieldID);
            cc.putOnField(isPlayerField, x.targets);
            cc.movement.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else
        {
            cc.movement.OnEndDrag(null);
        }
    }
    private bool IsExistTarget()
    {
        var x = SkillManager.instance.GetCardsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
        if (x.Count != 0) { return true; }
        return false;
    }
    private (bool passed,CardController[] targets) targetCheck(CardController cc, GameObject clickGameObject)
    {
        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc);
        clickGameObject?.TryGetComponent<CardController>(out c);
        if (cc.model.spellTarget == CardEntity.SpellTarget.enemyUnit)
        {
            var x = SkillManager.instance.GetCardsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
            if (x.Count == 0) { return (true, null); }
            if (c != null)
            {
                var y = x.Where(i => i.model.fieldID == c.model.fieldID);
                if(y.Count() == 0) { return (false, null); }
                else { return (true, y.ToArray()); }
            }
        }
        return (false, null);
    }
}
