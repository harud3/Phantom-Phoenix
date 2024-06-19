using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using Photon.Pun;

public class CardMovement : MonoBehaviourPunCallbacks, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent {  get; private set; } //�I�u�W�F�N�g�̐e
    public Transform recordDefaultParent { get; private set; } //��D����ړ������̈ʒu�ɓ�����������D�ɖ߂������ɏ��Ԃ�����ւ��Ȃ��悤�ɂ��邽�߁A�ړ��O�̐e���L�^

    [NonSerialized]
    public bool isDraggable; //�������邩�ǂ���

    int siblingIndex;
    void Start()
    {
        //null�P�A
        recordDefaultParent =  defaultParent = transform.parent;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; } //TODO
        siblingIndex = transform.GetSiblingIndex();

        //��D�̃J�[�h����hero��MP > �J�[�h�̃R�X�g�Ȃ瓮������
        //field�̃J�[�h�ōU���\�Ȃ瓮������
        CardController cardController = GetComponent<CardController>();
        if (!cardController.model.isPlayerCard) { return; } //TODO
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
        //�h���b�v�ł���悤��
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
        //�e��ύX DropPlace.cs����defaultParent���ύX����Ă���ꍇ�A�ړ��O�Ƃ͕ʂ̐e�ƂȂ�@��D��field
        transform.SetParent(defaultParent, false);
        if (recordDefaultParent == defaultParent) { transform.SetSiblingIndex(siblingIndex); }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    /// <summary>
    /// enemyAI�֌W�̏���
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public IEnumerator MoveToField(Transform field)
    {
        if(defaultParent is null) { defaultParent = transform.parent; } //EnemyAI����U����������null�G���[�΍�
        transform.SetParent(defaultParent.parent);
        transform.DOMove(field.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        defaultParent = field;
        transform.SetParent(defaultParent);
    }
    /// <summary>
    /// enemyAi�֌W�̏���
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator MoveToTarget(Transform target)
    {
        var currentPosition = transform.position;
        transform.SetParent(defaultParent.parent);
        transform.DOMove(target.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
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
        SendMoveField(fieldID);
    }
    public void SendMoveField(int fieldID)
    {
        photonView.RPC(nameof(MoveField), RpcTarget.Others, fieldID, siblingIndex);
    }
    [PunRPC]
    void MoveField(int fieldID, int handIndex)
    {
        StartCoroutine(GameManager.instance.MoveToField(handIndex, fieldID));
    }
}
