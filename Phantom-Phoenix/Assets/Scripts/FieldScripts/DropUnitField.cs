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
public class DropUnitField : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField]
    private bool isPlayerField; //player��enemy���@���O�ɃC���X�y�N�^�[��Őݒ�@�s��
    [SerializeField]
    private int _fieldID; //fieldID�����O�ɃC���X�y�N�^�[��Őݒ�@�s��
    [SerializeField]
    private GameObject HintMessage;
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

                cc.SummonOnField(fieldID);
            }
            
        }
        
    }
    /// <summary>
    ///�@���͑҂��@���͂�����΁A���ʂ̔���Ɉڂ�    �������ꂪ��Ԓx���Ǝv���܂�
    /// </summary>
    IEnumerator waitPlayerClick(CardController cc)
    {
        FieldManager.instance.ChangeSelectablePanelColor(fieldID, true); //�����\��t�B�[���h�̃p�l���F�ύX�@�ԁ���
        FieldManager.instance.SetSelectablePanel(new int[] { fieldID }.ToArray(), true); //�����\��t�B�[���h�̃p�l����\������
        HintMessage.SetActive(true); //�q���g�̕\��
        cc.gameObject.SetActive(false);�@//���ʂ����J�[�h�𖳌������� //�I�𒆂͒��݂��Ԃ̂܂܁A��\���ɂ�����

        //���͑҂�
        yield return new WaitUntil(() => Input.GetMouseButton(0));

        FieldManager.instance.ChangeSelectablePanelColor(fieldID, false);�@//�����\��t�B�[���h�̃p�l���F�ύX�@�΁���
        FieldManager.instance.SetSelectablePanel(Enumerable.Range(1, 12).ToArray(), false); //�t�B�[���h�̑I���\�p�l�����\���ɂ���
        FieldManager.instance.SetHeroSelectablePanel(Enumerable.Range(1, 2).ToArray(), false); //�t�B�[���h�̑I���\�p�l�����\���ɂ���
        HintMessage.SetActive(false);
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

        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card" || i.gameObject.tag == "Hero").FirstOrDefault().gameObject; //tag�Ŕ��f���邱�Ƃɂ����@���j�b�g��q�[���[���d�Ȃ��Ă邱�Ƃ͂Ȃ�
        if (TargetCheck(cc, clickedGameObject) is var x && x.passed)
        {
            //���j�b�g���t�B�[���h�ɏ������鏈��
            cc.movement.SetDefaultParent(this.transform, fieldID);
            cc.movement.SendMoveToField(fieldID, x.targetsByReceiver);
            cc.transform.SetParent(this.transform);

            cc.SummonOnField(fieldID, x.cctargets, x.hctarget);
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
    private bool IsExistTarget(CardController cc)
    {
        switch (cc.model.target)
        {
            case CardEntity.Target.unit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //�擾�����J�[�h�Q����fieldID���擾���A�Y���t�B�[���h�ɑI���\�p�l����\������
                        return true;
                    }
                    break;
                }
            case CardEntity.Target.playerUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //�擾�����J�[�h�Q����fieldID���擾���A�Y���t�B�[���h�ɑI���\�p�l����\������
                        return true;
                    }
                    break;
                }
            case CardEntity.Target.enemyUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //�擾�����J�[�h�Q����fieldID���擾���A�Y���t�B�[���h�ɑI���\�p�l����\������
                        return true;
                    }
                    break;
                }
            case CardEntity.Target.player:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //�擾�����J�[�h�Q����fieldID���擾���A�Y���t�B�[���h�ɑI���\�p�l����\������
                    }
                    FieldManager.instance.SetHeroSelectablePanel(new int[] { 1 }, true); //�����q�[���[�͊m��őΏۂƂȂ�
                    return true;
                }
            case CardEntity.Target.enemy:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //�擾�����J�[�h�Q����fieldID���擾���A�Y���t�B�[���h�ɑI���\�p�l����\������
                    }
                    FieldManager.instance.SetHeroSelectablePanel(new int[] { 2 }, true); //�G�q�[���[�͊m��őΏۂƂȂ�
                    return true;
                }
        }

        return false;
    }
    /// <summary>
    /// �Ώۂ̊m�F targetsByReceiver�͑ΐ푊��ɑ��M�������
    /// </summary>
    private (bool passed, CardController[] cctargets, HeroController hctarget, int[] targetsByReceiver) TargetCheck(CardController cc, GameObject clickGameObject)
    {
        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc); //��ꂽ�炢���ł���
        clickGameObject?.TryGetComponent<CardController>(out c); //��ꂽ�炢���ł���
        switch(cc.model.target){
            case CardEntity.Target.unit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray());
                    if (x.Count == 0) { return (true, null, null, null); }//�����ʂ邱�ƂȂ��C�����邯�ǁc
                    if (c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => FieldManager.instance.ChangeFieldID(i.model.thisFieldID)).ToArray()); }
                    }
                    break;
                }
            case CardEntity.Target.playerUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count == 0) { return (true, null, null, null); }//�����ʂ邱�ƂȂ��C�����邯�ǁc
                    if (c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => FieldManager.instance.ChangeFieldID(i.model.thisFieldID)).ToArray()); }//����ł͊Y������̍ő�1�����Ȃ����ǁA�����I���\�����������Ĕz��ɂ��Ă���
                    }
                    break;
                }
            case CardEntity.Target.enemyUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count == 0) { return (true, null, null, null); }//�����ʂ邱�ƂȂ��C�����邯�ǁc
                    if (c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => i.model.thisFieldID - 6).ToArray()); }//����ł͊Y������̍ő�1�����Ȃ����ǁA�����I���\�����������Ĕz��ɂ��Ă���
                                                                                                         //ex) targets�͑I�񂾑Ώۂł���A���M�Җڐ��œG��fieldID7�Ȃ�A��M�Җڐ��ł͖�����fieldID1�ƂȂ�
                    }
                    break;
                }
            case CardEntity.Target.player:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count != 0 && c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => i.model.thisFieldID - 6).ToArray()); }//����ł͊Y������̍ő�1�����Ȃ����ǁA�����I���\�����������Ĕz��ɂ��Ă���
                                                                                                                    //ex) targets�͑I�񂾑Ώۂł���A���M�Җڐ��Ŗ�����fieldID1�Ȃ�A��M�Җڐ��ł͓G��fieldID7�ƂȂ�
                    }
                    else if (hc != null)
                    {
                        return (true, null, hc, new int[] { 14 }); //14�Ԃ͓G�q�[���[�@��M�Ҍ����ɒl�����ւ��Ă���
                    }
                    break;
                }
            case CardEntity.Target.enemy:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count != 0 && c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => i.model.thisFieldID - 6).ToArray()); }//����ł͊Y������̍ő�1�����Ȃ����ǁA�����I���\�����������Ĕz��ɂ��Ă���
                                                                                                              //ex) targets�͑I�񂾑Ώۂł���A���M�Җڐ��œG��fieldID7�Ȃ�A��M�Җڐ��ł͖�����fieldID1�ƂȂ�
                    }
                    else if(hc != null)
                    {
                        return (true, null, hc, new int[] { 13 }); //13�Ԃ͖����q�[���[�@��M�Ҍ����ɒl�����ւ��Ă���
                    }
                    break;
                }
        }
        return (false, null, null, null);
    }
}
