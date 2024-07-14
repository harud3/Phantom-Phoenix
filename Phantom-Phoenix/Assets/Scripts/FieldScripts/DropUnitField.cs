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
public class DropUnitField : MonoBehaviourPunCallbacks, IDropHandler
{
    [SerializeField]
    private bool isPlayerField; //playerかenemyか　事前にインスペクター上で設定　不変
    [SerializeField]
    private int _fieldID; //fieldIDも事前にインスペクター上で設定　不変
    [SerializeField]
    private GameObject HintMessage;
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

                cc.SummonOnField(fieldID);
            }
            
        }
        
    }
    /// <summary>
    ///　入力待ち　入力があれば、効果の判定に移る    多分これが一番遅いと思います
    /// </summary>
    IEnumerator waitPlayerClick(CardController cc)
    {
        FieldManager.instance.ChangeSelectablePanelColor(fieldID, true); //召喚予定フィールドのパネル色変更　赤→緑
        FieldManager.instance.SetSelectablePanel(new int[] { fieldID }.ToArray(), true); //召喚予定フィールドのパネルを表示する
        HintMessage.SetActive(true); //ヒントの表示
        cc.gameObject.SetActive(false);　//効果を持つカードを無効化する //選択中は宙吊り状態のまま、非表示にしたい

        //入力待ち
        yield return new WaitUntil(() => Input.GetMouseButton(0));

        FieldManager.instance.ChangeSelectablePanelColor(fieldID, false);　//召喚予定フィールドのパネル色変更　緑→赤
        FieldManager.instance.SetSelectablePanel(Enumerable.Range(1, 12).ToArray(), false); //フィールドの選択可能パネルを非表示にする
        FieldManager.instance.SetHeroSelectablePanel(Enumerable.Range(1, 2).ToArray(), false); //フィールドの選択可能パネルを非表示にする
        HintMessage.SetActive(false);
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

        clickedGameObject = RayResult.Where(i => i.gameObject.tag == "Card" || i.gameObject.tag == "Hero").FirstOrDefault().gameObject; //tagで判断することにした　ユニットやヒーローが重なってることはない
        if (TargetCheck(cc, clickedGameObject) is var x && x.passed)
        {
            //ユニットをフィールドに召喚する処理
            cc.movement.SetDefaultParent(this.transform, fieldID);
            cc.movement.SendMoveToField(fieldID, x.targetsByReceiver);
            cc.transform.SetParent(this.transform);

            cc.SummonOnField(fieldID, x.cctargets, x.hctarget);
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
    private bool IsExistTarget(CardController cc)
    {
        switch (cc.model.target)
        {
            case CardEntity.Target.unit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //取得したカード群からfieldIDを取得し、該当フィールドに選択可能パネルを表示する
                        return true;
                    }
                    break;
                }
            case CardEntity.Target.playerUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //取得したカード群からfieldIDを取得し、該当フィールドに選択可能パネルを表示する
                        return true;
                    }
                    break;
                }
            case CardEntity.Target.enemyUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //取得したカード群からfieldIDを取得し、該当フィールドに選択可能パネルを表示する
                        return true;
                    }
                    break;
                }
            case CardEntity.Target.player:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //取得したカード群からfieldIDを取得し、該当フィールドに選択可能パネルを表示する
                    }
                    FieldManager.instance.SetHeroSelectablePanel(new int[] { 1 }, true); //味方ヒーローは確定で対象となる
                    return true;
                }
            case CardEntity.Target.enemy:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //取得したカード群からfieldIDを取得し、該当フィールドに選択可能パネルを表示する
                    }
                    FieldManager.instance.SetHeroSelectablePanel(new int[] { 2 }, true); //敵ヒーローは確定で対象となる
                    return true;
                }
        }

        return false;
    }
    /// <summary>
    /// 対象の確認 targetsByReceiverは対戦相手に送信される情報
    /// </summary>
    private (bool passed, CardController[] cctargets, HeroController hctarget, int[] targetsByReceiver) TargetCheck(CardController cc, GameObject clickGameObject)
    {
        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc); //取れたらいいですね
        clickGameObject?.TryGetComponent<CardController>(out c); //取れたらいいですね
        switch(cc.model.target){
            case CardEntity.Target.unit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray());
                    if (x.Count == 0) { return (true, null, null, null); }//ここ通ることない気がするけど…
                    if (c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => FieldManager.instance.ChangeFieldID(i.model.thisFieldID)).ToArray()); }
                    }
                    break;
                }
            case CardEntity.Target.playerUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count == 0) { return (true, null, null, null); }//ここ通ることない気がするけど…
                    if (c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => FieldManager.instance.ChangeFieldID(i.model.thisFieldID)).ToArray()); }//現状では該当するの最大1個しかないけど、複数選択可能化を見据えて配列にしておく
                    }
                    break;
                }
            case CardEntity.Target.enemyUnit:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count == 0) { return (true, null, null, null); }//ここ通ることない気がするけど…
                    if (c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => i.model.thisFieldID - 6).ToArray()); }//現状では該当するの最大1個しかないけど、複数選択可能化を見据えて配列にしておく
                                                                                                         //ex) targetsは選んだ対象であり、送信者目線で敵のfieldID7なら、受信者目線では味方のfieldID1となる
                    }
                    break;
                }
            case CardEntity.Target.player:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
                    if (x.Count != 0 && c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => i.model.thisFieldID - 6).ToArray()); }//現状では該当するの最大1個しかないけど、複数選択可能化を見据えて配列にしておく
                                                                                                                    //ex) targetsは選んだ対象であり、送信者目線で味方のfieldID1なら、受信者目線では敵のfieldID7となる
                    }
                    else if (hc != null)
                    {
                        return (true, null, hc, new int[] { 14 }); //14番は敵ヒーロー　受信者向けに値を入れ替えておく
                    }
                    break;
                }
            case CardEntity.Target.enemy:
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
                    if (x.Count != 0 && c != null)
                    {
                        var y = x.Where(i => i.model.thisFieldID == c.model.thisFieldID);
                        if (y.Count() == 0) { return (false, null, null, null); }
                        else { return (true, y.ToArray(), null, y.Select(i => i.model.thisFieldID - 6).ToArray()); }//現状では該当するの最大1個しかないけど、複数選択可能化を見据えて配列にしておく
                                                                                                              //ex) targetsは選んだ対象であり、送信者目線で敵のfieldID7なら、受信者目線では味方のfieldID1となる
                    }
                    else if(hc != null)
                    {
                        return (true, null, hc, new int[] { 13 }); //13番は味方ヒーロー　受信者向けに値を入れ替えておく
                    }
                    break;
                }
        }
        return (false, null, null, null);
    }
}
