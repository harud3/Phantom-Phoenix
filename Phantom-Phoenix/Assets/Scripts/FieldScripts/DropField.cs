using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

/// <summary>
/// �J�[�h���t�B�[���h�Ƀh���b�v�������̏��� �e�t�B�[���h�ɂ��Ă���
/// </summary>
public class DropField : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField]
    private bool isPlayerField; //player��enemy���@���O�ɃC���X�y�N�^�[��Őݒ�@�s��
    [SerializeField]
    private int _fieldID; //fieldID�����O�ɃC���X�y�N�^�[��Őݒ�@�s��
    public int fieldID { get { return _fieldID; } }
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; } //�����̃^�[���ł͂Ȃ��̂ɒu�����Ƃ��Ȃ���

        //�t�B�[���h�ɒu��������́A
        //���j�b�g�J�[�h�ŁA
        //�t�B�[���h�ƃJ�[�h���F�D�ł���A (p&p || e&e)
        //��D�̃J�[�h�ł���A
        //�h���b�O�\�ł���A
        //�t�B�[���h�ɑ��̃J�[�h���u����Ă��Ȃ����@�ł���
        CardController cc = eventData.pointerDrag.GetComponent<CardController>();
        if (cc == null�@|| cc.model.category == CardEntity.Category.spell 
            || isPlayerField != cc.model.isPlayerCard || cc.model.isFieldCard)
        {
            return;
        }

        if (cc.movement.isDraggable && this.transform.childCount == 0)
        {
            if (cc.model.HasSelectSpeciallSkill && IsExistTarget(cc)) //�I�����K�v�ȃX�L�������ŁA�ΏۂƂȂ肤���₪����Ȃ�A���͑҂����J�n����
            {
                StartCoroutine(waitPlayerClick(cc));
            }
            else //������
            {
                //���j�b�g���t�B�[���h�ɏ������鏈��
                cc.movement.SetDefaultParent(this.transform, fieldID);
                cc.movement.SendMoveToField(fieldID);

                cc.SummonOnField(isPlayerField, fieldID);
            }
            
        }
        
    }
    /// <summary>
    ///�@���͑҂��@���͂�����΁A���ʂ̔���Ɉڂ�    �������ꂪ��Ԓx���Ǝv���܂�
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    IEnumerator waitPlayerClick(CardController cc)
    {
        cc.gameObject.SetActive(false);�@//���ʂ����J�[�h�𖳌�������
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        cc.gameObject.SetActive(true); //���ʂ����J�[�h��L��������



        //���R�s�y�@���[���h���W���Ƀn�}���Ēɂ��ڂ�����
        GameObject clickedGameObject = null;

        //RaycastAll�̈����iPointerEventData�j�쐬
        PointerEventData pointData = new PointerEventData(EventSystem.current);

        //RaycastAll�̌��ʊi�[�pList
        List<RaycastResult> RayResult = new List<RaycastResult>();

        //PointerEventData�Ƀ}�E�X�̈ʒu���Z�b�g
        pointData.position = Input.mousePosition;
        //RayCast�i�X�N���[�����W�j
        EventSystem.current.RaycastAll(pointData, RayResult);
        //���R�s�y

        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card").FirstOrDefault().gameObject; //tag�Ŕ��f���邱�Ƃɂ����@���j�b�g��q�[���[���d�Ȃ��Ă邱�Ƃ͂Ȃ�
        if (TargetCheck(cc, clickedGameObject) is var x && x.passed)
        {
            //���j�b�g���t�B�[���h�ɏ������鏈��
            cc.movement.SetDefaultParent(this.transform, fieldID);
            cc.movement.SendMoveToField(fieldID, x.targetsByReceiver);
            cc.transform.SetParent(this.transform);

            cc.SummonOnField(isPlayerField, fieldID, x.cctargets);
            cc.movement.GetComponent<CanvasGroup>().blocksRaycasts = true; //OnEndDrag�ł�鏈�������ɂ���Ă���
        }
        else
        {
            cc.movement.OnEndDrag(null); //�܂����ʂ�Ǝv���ĂȂ������c������
        }
    }
    /// <summary>
    /// �ΏۂƂȂ肤���₪���邩�m�F
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    private bool IsExistTarget(CardController cc)
    {
        if (cc.model.target == CardEntity.Target.enemyUnit)
        {
            var x = FieldManager.instance.GetUnitsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
            if (x.Count != 0) { return true; }
        }
        return false;
    }
    /// <summary>
    /// �Ώۂ̊m�F targetsByReceiver�͑ΐ푊��ɑ��M�������
    /// </summary>
    /// <param name="cc"></param>
    /// <param name="clickGameObject"></param>
    /// <returns></returns>
    private (bool passed, CardController[] cctargets,int[] targetsByReceiver) TargetCheck(CardController cc, GameObject clickGameObject)
    {
        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc); //��ꂽ�炢���ł���
        clickGameObject?.TryGetComponent<CardController>(out c); //��ꂽ�炢���ł���
        if (cc.model.target == CardEntity.Target.enemyUnit)
        {
            var x = FieldManager.instance.GetUnitsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
            if (x.Count == 0) { return (true, null, null); }//�����ʂ邱�ƂȂ��C�����邯�ǁc
            if (c != null)
            {
                var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                if(y.Count() == 0) { return (false, null, null); }
                else { return (true, y.ToArray(), y.Select(i => i.model.thisFieldID - 6).ToArray() ); }//����ł͊Y������̍ő�1�����Ȃ����ǁA�����I�����������Ĕz��ɂ��Ă���
                //ex) targets�͑I�񂾑Ώۂł���A���M�Җڐ��œG��7�Ȃ�A��M�Җڐ��ł͖�����1�ƂȂ�
            }
        }
        return (false, null, null);
    }
}
