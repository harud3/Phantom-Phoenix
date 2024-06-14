using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [NonSerialized]
    public Transform defaultParent;

    public bool isDraggable;
    public void OnBeginDrag(PointerEventData eventData)
    {
        int siblingIndex = transform.GetSiblingIndex();
        CardController cardController = GetComponent<CardController>();
        if(!cardController.model.isFieldCard && cardController.model.cost <= GameManager.instance.GetPlayerMP())
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
        defaultParent = transform.parent;
        transform.SetParent(defaultParent.parent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) { return; }
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) { return; }
        transform.SetParent(defaultParent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    public IEnumerator MoveToFiled(Transform field)
    {
        Debug.Log($"{defaultParent.name}‚ÉˆÚ“®");
        transform.SetParent(defaultParent.parent);
        transform.DOMove(field.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        defaultParent = field;
        transform.SetParent(defaultParent);
    }
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
    void Start()
    {
        defaultParent = transform.parent;
    }
}
