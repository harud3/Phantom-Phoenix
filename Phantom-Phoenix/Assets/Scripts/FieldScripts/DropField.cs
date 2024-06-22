using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

/// <summary>
/// カードをフィールドにドロップした時の処理 各フィールドについている
/// </summary>
public class DropField : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField]
    private bool isPlayerField; //playerかenemyか　事前にインスペクター上で設定　不変
    [SerializeField]
    private int _fieldID; //fieldIDも事前にインスペクター上で設定　不変
    public int fieldID { get { return _fieldID; } }
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; } //自分のターンではないのに置こうとしないで

        //フィールドに置ける条件は、
        //ユニットカードで、
        //フィールドとカードが友好であり、 (p&p || e&e)
        //手札のカードであり、
        //ドラッグ可能であり、
        //フィールドに他のカードが置かれていない時　である
        CardController cc = eventData.pointerDrag.GetComponent<CardController>();
        if (cc == null　|| cc.model.category == CardEntity.Category.spell 
            || isPlayerField != cc.model.isPlayerCard || cc.model.isFieldCard)
        {
            return;
        }

        if (cc.movement.isDraggable && this.transform.childCount == 0)
        {
            if (cc.model.HasSelectSpeciallSkill && IsExistTarget(cc)) //選択が必要なスキル持ちで、対象となりうる候補がいるなら、入力待ちを開始する
            {
                StartCoroutine(waitPlayerClick(cc));
            }
            else //大抵こっち
            {
                //ユニットをフィールドに召喚する処理
                cc.movement.SetDefaultParent(this.transform, fieldID);
                cc.movement.SendMoveToField(fieldID);

                cc.SummonOnField(isPlayerField, fieldID);
            }
            
        }
        
    }
    /// <summary>
    ///　入力待ち　入力があれば、効果の判定に移る    多分これが一番遅いと思います
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    IEnumerator waitPlayerClick(CardController cc)
    {
        cc.gameObject.SetActive(false);　//効果を持つカードを無効化する
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        cc.gameObject.SetActive(true); //効果を持つカードを有効化する



        //↓コピペ　ワールド座標沼にハマって痛い目を見た
        GameObject clickedGameObject = null;

        //RaycastAllの引数（PointerEventData）作成
        PointerEventData pointData = new PointerEventData(EventSystem.current);

        //RaycastAllの結果格納用List
        List<RaycastResult> RayResult = new List<RaycastResult>();

        //PointerEventDataにマウスの位置をセット
        pointData.position = Input.mousePosition;
        //RayCast（スクリーン座標）
        EventSystem.current.RaycastAll(pointData, RayResult);
        //↑コピペ

        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card").FirstOrDefault().gameObject; //tagで判断することにした　ユニットやヒーローが重なってることはない
        if (TargetCheck(cc, clickedGameObject) is var x && x.passed)
        {
            //ユニットをフィールドに召喚する処理
            cc.movement.SetDefaultParent(this.transform, fieldID);
            cc.movement.SendMoveToField(fieldID, x.targetsByReceiver);
            cc.transform.SetParent(this.transform);

            cc.SummonOnField(isPlayerField, fieldID, x.cctargets);
            cc.movement.GetComponent<CanvasGroup>().blocksRaycasts = true; //OnEndDragでやる処理を代わりにやっておく
        }
        else
        {
            cc.movement.OnEndDrag(null); //まさか通ると思ってなかった…動いた
        }
    }
    /// <summary>
    /// 対象となりうる候補がいるか確認
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    private bool IsExistTarget(CardController cc)
    {
        if (cc.model.target == CardEntity.Target.enemyUnit)
        {
            var x = FieldManager.instance.GetUnitsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
            if (x.Count != 0) { return true; }
        }
        return false;
    }
    /// <summary>
    /// 対象の確認 targetsByReceiverは対戦相手に送信される情報
    /// </summary>
    /// <param name="cc"></param>
    /// <param name="clickGameObject"></param>
    /// <returns></returns>
    private (bool passed, CardController[] cctargets,int[] targetsByReceiver) TargetCheck(CardController cc, GameObject clickGameObject)
    {
        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc); //取れたらいいですね
        clickGameObject?.TryGetComponent<CardController>(out c); //取れたらいいですね
        if (cc.model.target == CardEntity.Target.enemyUnit)
        {
            var x = FieldManager.instance.GetUnitsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 });
            if (x.Count == 0) { return (true, null, null); }//ここ通ることない気がするけど…
            if (c != null)
            {
                var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                if(y.Count() == 0) { return (false, null, null); }
                else { return (true, y.ToArray(), y.Select(i => i.model.thisFieldID - 6).ToArray() ); }//現状では該当するの最大1個しかないけど、複数選択を見据えて配列にしておく
                //ex) targetsは選んだ対象であり、送信者目線で敵の7なら、受信者目線では味方の1となる
            }
        }
        return (false, null, null);
    }
}
