using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// カードをfieldにドロップした時の処理
/// </summary>
public class DropField : MonoBehaviour, IDropHandler
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
        CardController cardController = eventData.pointerDrag.GetComponent<CardController>();
        if (cardController == null　|| cardController.model.category == CardEntity.Category.spell 
            || isPlayerField != cardController.model.isPlayerCard || cardController.model.isFieldCard)
        {
            return;
        }

        if (cardController.movement.isDraggable && this.transform.childCount == 0)
        {
            //カードをfieldに置く処理
            cardController.movement.SetDefaultParent(this.transform);
            //modelにfieldIDを設定し、fieldに置く時の処理を行う
            cardController.MoveField(fieldID);
            cardController.putOnField(isPlayerField);
            
        }
        
    }
}
