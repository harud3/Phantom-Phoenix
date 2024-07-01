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
/// カードの挙動 Cardプレハブについてる
/// </summary>
public class CardMovement : MonoBehaviourPunCallbacks, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent {  get; private set; } //オブジェクトの親
    public Transform recordDefaultParent { get; private set; } //オブジェクト移動前の親　
    int siblingIndex; //手札から移動→他の位置に動かさなかった時→手札に戻った時に順番が入れ替わらないようにするため

    [NonSerialized]
    public bool isDraggable; //動かせるかどうか

    
    void Start()
    {
        //nullケア
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

        //マリガン処理
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

        //マリガン処理
        if (cardController.model.isMulliganCard)
        {
            AudioManager.instance.SoundCardMove();
            cardController.SetIsMulligan(!cardController.model.isMulligan);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {

        if (!GameManager.instance.isPlayerTurn) { isDraggable = false;  return; } //自分のターンではないのに動かそうとするのは見過ごせない
        siblingIndex = transform.GetSiblingIndex();
        CardController cardController = GetComponent<CardController>();

        //手札のカードかつ、ヒーローのMP > カードのコストなら動かせる
        //フィールドのカードで攻撃可能なら動かせる
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

        //親を記録
        recordDefaultParent =  defaultParent = transform.parent;
        //移動の見た目の問題で、親を親の親に変更
        transform.SetParent(defaultParent.parent, false);
        //DropField.csが反応するように
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
        //親を変更 DropPlace.csからdefaultParentが変更されている場合、移動前とは別の親となる　手札→フィールド
        
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
    /// 一方通行の移動演出
    /// </summary>
    /// <param name="targetArea"></param>
    /// <returns></returns>
    public IEnumerator MoveToArea(Transform targetArea)
    {
        if(defaultParent is null) { defaultParent = transform.parent; } //nullエラー対策
        transform.SetParent(defaultParent);
        transform.DOMove(targetArea.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        defaultParent = targetArea;
        transform.SetParent(defaultParent);
        transform.SetSiblingIndex(siblingIndex-1);　//最大値の場合、自動レイアウトされないので、1度他の位置を経由する
        transform.SetSiblingIndex(siblingIndex);
    }
    /// <summary>
    /// 往復の移動演出 バトルで使うので途中でサウンドも再生
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public IEnumerator MoveToTarget(Transform target)
    {
        var currentPosition = transform.position;
        transform.SetParent(defaultParent.parent);
        transform.DOMove(target.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        AudioManager.instance.SoundCardAttack(); //targetいる == バトル です
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        transform.SetParent(defaultParent);
    }

    /// <summary>
    /// DropPlace.csからのdefaultParent変更用
    /// </summary>
    /// <param name="dropPlace"></param>
    public void SetDefaultParent(Transform dropPlace, int fieldID)
    {
        defaultParent = dropPlace;
        transform.SetParent(defaultParent, false);
    }
    /// <summary>
    /// 手札に出たことを対戦相手に送信　cardMovement.csで取得したsiblingIndexが必要なのでcardMovement.cs内にある
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
