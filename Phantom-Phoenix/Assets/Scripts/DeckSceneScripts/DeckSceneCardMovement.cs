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
/// デッキ編成画面のカードの挙動
/// </summary>
public class DeckSceneCardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent {  get; private set; } //オブジェクトの親
    public Transform recordDefaultParent { get; private set; } //同じ位置に戻った時に順番が入れ替わらないようにするため、移動前の親を記録

    [SerializeField]
    GameObject cardPrefab;
    int siblingIndex;
    void Start()
    {
        //nullケア
        recordDefaultParent =  defaultParent = transform.parent;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        AudioManager.instance.SoundCardMove();
        //親を記録
        recordDefaultParent = defaultParent = transform.parent;

        if(recordDefaultParent.name == "PanelDeck") //デッキから動いたものは消される運命にある
        {
            Destroy(this.gameObject); return;
        }

        if (recordDefaultParent.name == "PanelStock") //カード一覧から動かした場合、その場にコピーを作成する
        {
            siblingIndex = transform.GetSiblingIndex();
            DeckSceneCardController card = Instantiate(cardPrefab, this.recordDefaultParent).GetComponent<DeckSceneCardController>();
            card.Init(this.GetComponent<DeckSceneCardController>().model.cardID);
            card.transform.SetSiblingIndex(siblingIndex);
        }

        //移動の見た目の問題で、親を親の親に変更
        transform.SetParent(defaultParent.parent, false);
        //ドロップできるように
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //ドラッグに追従する
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(defaultParent, false);
        if (recordDefaultParent.name == "PanelStock" && defaultParent.name == "PanelStock")
        {
            Destroy(this.gameObject); //パネルには既にコピーがいるので消さないと増え続けることになる
        }
        else
        {
            SortPanelDeck(); //デッキ並べ替え
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    /// <summary>
    /// DropPlace.csからのdefaultParent変更用
    /// </summary>
    /// <param name="dropPlace"></param>
    public void SetDefaultParent(Transform dropPlace)
    {
        defaultParent = dropPlace;
    }
    /// <summary>
    /// デッキに対して、CardID昇順で並び替えを行う
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
