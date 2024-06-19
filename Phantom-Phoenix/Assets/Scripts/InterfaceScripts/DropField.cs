using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// カードをfieldにドロップした時の処理
/// </summary>
public class DropField : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField]
    private bool isPlayerField; //playerかenemyか　事前にインスペクター上で設定
    [SerializeField]
    private int _fieldID; //fieldIDも持っておく
    public int fieldID { get { return _fieldID; } }
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; }//TODO

        //fieldに置ける条件は、
        //cardConrollerがあり、
        //unitで、
        //playerのもの同士かenemyのもの同士であり、
        //手札のカードであり、
        //ドラッグ可能であり、
        //fieldに他のカードが置かれていない時　である
        CardController cc = eventData.pointerDrag.GetComponent<CardController>();
        if (cc == null　|| cc.model.category == CardEntity.Category.spell 
            || isPlayerField != cc.model.isPlayerCard || cc.model.isFieldCard)
        {
            return;
        }

        if (cc.movement.isDraggable && this.transform.childCount == 0)
        {
            //カードをfieldに置く処理
            cc.movement.SetDefaultParent(this.transform, fieldID);
            //modelにfieldIDを設定し、fieldに置く時の処理を行う
            cc.MoveField(fieldID);
            cc.putOnField(isPlayerField);
        }
        
    }
}
