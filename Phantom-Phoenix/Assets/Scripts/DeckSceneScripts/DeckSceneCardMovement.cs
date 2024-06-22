using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
/// <summary>
/// �f�b�L�Ґ���ʂ̃J�[�h�̋���
/// </summary>
public class DeckSceneCardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent {  get; private set; } //�I�u�W�F�N�g�̐e
    public Transform recordDefaultParent { get; private set; } //�����ʒu�ɖ߂������ɏ��Ԃ�����ւ��Ȃ��悤�ɂ��邽�߁A�ړ��O�̐e���L�^

    [SerializeField]
    GameObject cardPrefab;
    int siblingIndex;
    void Start()
    {
        //null�P�A
        recordDefaultParent =  defaultParent = transform.parent;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        AudioManager.instance.SoundCardMove();
        //�e���L�^
        recordDefaultParent = defaultParent = transform.parent;

        if(recordDefaultParent.name == "PanelDeck") //�f�b�L���瓮�������̂͏������^���ɂ���
        {
            Destroy(this.gameObject); return;
        }

        if (recordDefaultParent.name == "PanelStock") //�J�[�h�ꗗ���瓮�������ꍇ�A���̏�ɃR�s�[���쐬����
        {
            siblingIndex = transform.GetSiblingIndex();
            DeckSceneCardController card = Instantiate(cardPrefab, this.recordDefaultParent).GetComponent<DeckSceneCardController>();
            card.Init(this.GetComponent<DeckSceneCardController>().model.cardID);
            card.transform.SetSiblingIndex(siblingIndex);
        }

        //�ړ��̌����ڂ̖��ŁA�e��e�̐e�ɕύX
        transform.SetParent(defaultParent.parent, false);
        //�h���b�v�ł���悤��
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //�h���b�O�ɒǏ]����
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(defaultParent, false);
        if (recordDefaultParent.name == "PanelStock" && defaultParent.name == "PanelStock")
        {
            Destroy(this.gameObject); //�p�l���ɂ͊��ɃR�s�[������̂ŏ����Ȃ��Ƒ��������邱�ƂɂȂ�
        }
        else
        {
            SortPanelDeck(); //�f�b�L���בւ�
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    /// <summary>
    /// DropPlace.cs�����defaultParent�ύX�p
    /// </summary>
    /// <param name="dropPlace"></param>
    public void SetDefaultParent(Transform dropPlace)
    {
        defaultParent = dropPlace;
    }
    /// <summary>
    /// �f�b�L�ɑ΂��āACardID�����ŕ��ёւ����s��
    /// </summary>
    private void SortPanelDeck()
    {
        var children = new List<Transform> { };
        foreach (Transform t in transform.parent)
        {
            children.Add(t);
        }
        var siblingIndex = 0;
        foreach (var sortedItem in children.OrderBy(x => x.GetComponent<DeckSceneCardController>().model.cardID))
        {
            sortedItem.SetSiblingIndex(siblingIndex++);
        }
    }
}
