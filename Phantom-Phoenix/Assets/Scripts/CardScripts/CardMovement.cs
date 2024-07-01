using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using Photon.Pun;
using Unity.VisualScripting;

/// <summary>
/// �J�[�h�̋��� Card�v���n�u�ɂ��Ă�
/// </summary>
public class CardMovement : MonoBehaviourPunCallbacks, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent {  get; private set; } //�I�u�W�F�N�g�̐e
    public Transform recordDefaultParent { get; private set; } //�I�u�W�F�N�g�ړ��O�̐e�@
    int siblingIndex; //��D����ړ������̈ʒu�ɓ������Ȃ�����������D�ɖ߂������ɏ��Ԃ�����ւ��Ȃ��悤�ɂ��邽��

    [NonSerialized]
    public bool isDraggable; //�������邩�ǂ���

    
    void Start()
    {
        //null�P�A
        recordDefaultParent =  defaultParent = transform.parent;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CardController cardController = GetComponent<CardController>();

        if (!cardController.model.isPlayerCard || cardController.model.isMulliganCard || cardController.model.isFieldCard)
        {
            return;
        }
        else
        {
            transform.localScale = Vector3.one * 1.4f;

        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CardController cardController = GetComponent<CardController>();

        //�}���K������
        if (!cardController.model.isPlayerCard ||  cardController.model.isMulliganCard || cardController.model.isFieldCard)
        {
            return;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CardController cardController = GetComponent<CardController>();

        //�}���K������
        if (cardController.model.isMulliganCard)
        {
            AudioManager.instance.SoundCardMove();
            cardController.SetIsMulligan(!cardController.model.isMulligan);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {

        if (!GameManager.instance.isPlayerTurn) { isDraggable = false;  return; } //�����̃^�[���ł͂Ȃ��̂ɓ��������Ƃ���̂͌��߂����Ȃ�
        siblingIndex = transform.GetSiblingIndex();
        CardController cardController = GetComponent<CardController>();

        //��D�̃J�[�h���A�q�[���[��MP > �J�[�h�̃R�X�g�Ȃ瓮������
        //�t�B�[���h�̃J�[�h�ōU���\�Ȃ瓮������
        if (cardController.model.isMulliganCard || !cardController.model.isPlayerCard) { isDraggable = false;  return; }
        if(!cardController.model.isFieldCard && cardController.model.cost <= GameManager.instance.GetHeroMP(cardController.model.isPlayerCard))
        {
            isDraggable = true;
        }
        else if(cardController.model.isFieldCard && cardController.model.canAttack)
        {
            isDraggable = true;
        }
        else
        {
            isDraggable = false;
        }
        if (!isDraggable) { return; }

        //�e���L�^
        recordDefaultParent =  defaultParent = transform.parent;
        //�ړ��̌����ڂ̖��ŁA�e��e�̐e�ɕύX
        transform.SetParent(defaultParent.parent, false);
        //DropField.cs����������悤��
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) { return; }
        //�h���b�O�ɒǏ]����
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) { return; }
        //�e��ύX DropPlace.cs����defaultParent���ύX����Ă���ꍇ�A�ړ��O�Ƃ͕ʂ̐e�ƂȂ�@��D���t�B�[���h
        
        if (recordDefaultParent == defaultParent) { 
            StartCoroutine(MoveToArea(recordDefaultParent));
        }
        else
        {
            transform.SetParent(defaultParent, false);
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    /// <summary>
    /// ����ʍs�̈ړ����o
    /// </summary>
    /// <param name="targetArea"></param>
    /// <returns></returns>
    public IEnumerator MoveToArea(Transform targetArea)
    {
        if(defaultParent is null) { defaultParent = transform.parent; } //null�G���[�΍�
        transform.SetParent(defaultParent);
        transform.DOMove(targetArea.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        defaultParent = targetArea;
        transform.SetParent(defaultParent);
        transform.SetSiblingIndex(siblingIndex-1);�@//�ő�l�̏ꍇ�A�������C�A�E�g����Ȃ��̂ŁA1�x���̈ʒu���o�R����
        transform.SetSiblingIndex(siblingIndex);
    }
    /// <summary>
    /// �����̈ړ����o �o�g���Ŏg���̂œr���ŃT�E���h���Đ�
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator MoveToTarget(Transform target)
    {
        var currentPosition = transform.position;
        transform.SetParent(defaultParent.parent);
        transform.DOMove(target.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        AudioManager.instance.SoundCardAttack(); //target���� == �o�g�� �ł�
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        transform.SetParent(defaultParent);
    }

    /// <summary>
    /// DropPlace.cs�����defaultParent�ύX�p
    /// </summary>
    /// <param name="dropPlace"></param>
    public void SetDefaultParent(Transform dropPlace, int fieldID)
    {
        defaultParent = dropPlace;
        transform.SetParent(defaultParent, false);
    }
    /// <summary>
    /// ��D�ɏo�����Ƃ�ΐ푊��ɑ��M�@cardMovement.cs�Ŏ擾����siblingIndex���K�v�Ȃ̂�cardMovement.cs���ɂ���
    /// </summary>
    /// <param name="fieldID"></param>
    /// <param name="targets"></param>
    public void SendMoveToField(int fieldID, int[] targetsByReceiver = null)
    {
        if (GameDataManager.instance.isOnlineBattle)
        {
            GameManager.instance.SendMoveToField(fieldID, siblingIndex, targetsByReceiver);
        }
    }
}
