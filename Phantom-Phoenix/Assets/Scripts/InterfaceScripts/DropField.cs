using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor.Experimental.GraphView;
using System.Linq;

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
            if (cc.model.HasSelectSpeciallSkill && IsExistTarget())
            {
                StartCoroutine(waitPlayerClick(cc));
            }
            else
            {
                //カードをfieldに置く処理
                cc.movement.SetDefaultParent(this.transform, fieldID);
                //modelにfieldIDを設定し、fieldに置く時の処理を行う
                cc.MoveField(fieldID);
                cc.putOnField(isPlayerField);
            }
            
        }
        
    }
    IEnumerator waitPlayerClick(CardController cc)
    {
        cc.gameObject.SetActive(false);
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        cc.gameObject.SetActive(true);
        GameObject clickedGameObject = null;

        //RaycastAllの引数（PointerEventData）作成
        PointerEventData pointData = new PointerEventData(EventSystem.current);

        //RaycastAllの結果格納用List
        List<RaycastResult> RayResult = new List<RaycastResult>();

        //PointerEventDataにマウスの位置をセット
        pointData.position = Input.mousePosition;
        //RayCast（スクリーン座標）
        EventSystem.current.RaycastAll(pointData, RayResult);
        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card").FirstOrDefault().gameObject;
        if (targetCheck(cc, clickedGameObject) is var x && x.passed)
        {
            //カードをfieldに置く処理
            cc.movement.SetDefaultParent(this.transform, fieldID, x.targets.Select(i => i.model.fieldID).ToArray());
            cc.transform.SetParent(this.transform);
            //modelにfieldIDを設定し、fieldに置く時の処理を行う
            cc.MoveField(fieldID);
            cc.putOnField(isPlayerField, x.targets);
            cc.movement.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else
        {
            cc.movement.OnEndDrag(null);
        }
    }
    private bool IsExistTarget()
    {
        var x = SkillManager.instance.GetCardsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
        if (x.Count != 0) { return true; }
        return false;
    }
    private (bool passed,CardController[] targets) targetCheck(CardController cc, GameObject clickGameObject)
    {
        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc);
        clickGameObject?.TryGetComponent<CardController>(out c);
        if (cc.model.spellTarget == CardEntity.SpellTarget.enemyUnit)
        {
            var x = SkillManager.instance.GetCardsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
            if (x.Count == 0) { return (true, null); }
            if (c != null)
            {
                var y = x.Where(i => i.model.fieldID == c.model.fieldID);
                if(y.Count() == 0) { return (false, null); }
                else { return (true, y.ToArray()); }
            }
        }
        return (false, null);
    }
}
