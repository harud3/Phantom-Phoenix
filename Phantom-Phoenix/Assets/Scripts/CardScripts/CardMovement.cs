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
    public Transform defaultParent {  get; private set; } //オブジェクトの親
    public Transform recordDefaultParent { get; private set; } //手札から移動→他の位置に動かさなかった時→手札に戻った時に順番が入れ替わらないようにするため、移動前の親を記録

    [NonSerialized]
    public bool isDraggable; //動かせるかどうか

    int siblingIndex;
    void Start()
    {
        //nullケア
        recordDefaultParent =  defaultParent = transform.parent;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { isDraggable = false;  return; } //TODO
        siblingIndex = transform.GetSiblingIndex();

        //手札のカードかつheroのMP > カードのコストなら動かせる
        //fieldのカードで攻撃可能なら動かせる
        CardController cardController = GetComponent<CardController>();
        if (!cardController.model.isPlayerCard) { Debug.Log("koko"); isDraggable = false;  return; }
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

        //親を記録
        recordDefaultParent =  defaultParent = transform.parent;
        //移動の見た目の問題で、親を親の親に変更
        transform.SetParent(defaultParent.parent, false);
        //ドロップできるように
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) { return; }
        //ドラッグに追従する
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) { return; }
        //親を変更 DropPlace.csからdefaultParentが変更されている場合、移動前とは別の親となる　手札→field
        
        if (recordDefaultParent == defaultParent) { 
            StartCoroutine(MoveToField(defaultParent));
        }
        else
        {
            transform.SetParent(defaultParent, false);
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    /// <summary>
    /// enemyAI関係の処理
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public IEnumerator MoveToField(Transform field)
    {
        if(defaultParent is null) { defaultParent = transform.parent; } //EnemyAIが先攻だった時のnullエラー対策
        transform.SetParent(defaultParent);
        transform.DOMove(field.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        defaultParent = field;
        transform.SetParent(defaultParent);
        transform.SetSiblingIndex(siblingIndex);
    }
    /// <summary>
    /// enemyAi関係の処理
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator MoveToTarget(Transform target)
    {
        var currentPosition = transform.position;
        transform.SetParent(defaultParent.parent);
        transform.DOMove(target.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        AudioManager.instance.SoundCardAttack();
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        transform.SetParent(defaultParent);
    }

    /// <summary>
    /// DropPlace.csからのdefaultParent変更用
    /// </summary>
    /// <param name="dropPlace"></param>
    public void SetDefaultParent(Transform dropPlace, int fieldID, int[] targets = null)
    {
        defaultParent = dropPlace;
        if (GameDataManager.instance.isOnlineBattle)
        {
            GameManager.instance.SendMoveField(fieldID, siblingIndex, targets);
        }
    }
    
}
