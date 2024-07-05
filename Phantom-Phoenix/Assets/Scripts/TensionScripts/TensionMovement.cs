using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

public class TensionMovement : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private GameObject HintMessage;
    public void OnPointerDown(PointerEventData eventData)
    {
        TensionController tensionController = GetComponent<TensionController>();

        if (!tensionController.model.isPlayer) { return; }
        //自分のターンに自分のテンションを操作できる　また、ターン中にテンションを使用しているとテンションの使用はできない
        //ただし、テンション3はテンションスペルを使用できる状態なので操作可能
        if (GameManager.instance.isPlayerTurn == tensionController.model.isPlayer)
        {
            if (tensionController.model.tension == 3)
            {
                switch (tensionController.model.tensionID)
                {
                    case 1: //elf
                        tensionController.UseTensionSpell<Controller>(null);
                        if (GameDataManager.instance.isOnlineBattle) //ここに入ってきている時点でccかhcのどちらかは対象となっている
                        {
                            GameManager.instance.SendUseTensionSpell(0);
                        }
                        break;
                    case 2: //witch
                        if (IsExistTarget(2))
                        {
                            StartCoroutine(waitPlayerClick(tensionController));
                        }
                        break;
                    case 3: //king
                        tensionController.UseTensionSpell<Controller>(null);
                        if (GameDataManager.instance.isOnlineBattle) //ここに入ってきている時点でccかhcのどちらかは対象となっている
                        {
                            GameManager.instance.SendUseTensionSpell(0);
                        }
                        break;
                    case 4: //demon
                        tensionController.UseTensionSpell<Controller>(null);
                        if (GameDataManager.instance.isOnlineBattle) //ここに入ってきている時点でccかhcのどちらかは対象となっている
                        {
                            GameManager.instance.SendUseTensionSpell(0);
                        }
                        break;
                    case 5: //knight
                        if (IsExistTarget(5))
                        {
                            StartCoroutine(waitPlayerClick(tensionController));
                        }
                        break;
                }

            }
            else if (!tensionController.model.isTensionUsedThisTurn)
            {
                tensionController.UseTensionCard();
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendUseTensionCard();
                }
            }

        }
    }
    /// <summary>
    ///　入力待ち　入力があれば、効果の判定に移る    多分これが一番遅いと思います
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    IEnumerator waitPlayerClick(TensionController tc)
    {
        HintMessage.SetActive(true); //ヒントの表示

        yield return null;
        //入力待ち
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        FieldManager.instance.SetSelectablePanel(Enumerable.Range(1, 12).ToArray(), false); //フィールドの選択可能パネルを非表示にする
        FieldManager.instance.SetHeroSelectablePanel(Enumerable.Range(1, 2).ToArray(), false);
        HintMessage.SetActive(false);

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
        if (TargetCheck(clickedGameObject, tc) is var x && x.passed)
        {
            if (x.cctarget != null)
            {
                tc.UseTensionSpell(x.cctarget);

            }
            else if (x.hctarget != null)
            {
                tc.UseTensionSpell(x.hctarget);
            }
            if (GameDataManager.instance.isOnlineBattle) //ここに入ってきている時点でccかhcのどちらかは対象となっている
            {
                GameManager.instance.SendUseTensionSpell(x.targetFieldIDByReceiver);
            }
        }
    }
    /// <summary>
    /// 対象となりうる候補がいるか確認
    /// </summary>
    /// <param name="cc"></param>
    /// <returns></returns>
    private bool IsExistTarget(int tensionID)
    {
        switch (tensionID)
        {
            case 2: //witch
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true); //取得したカード群からfieldIDを取得し、該当フィールドに選択可能パネルを表示する

                    }
                    FieldManager.instance.SetHeroSelectablePanel(Enumerable.Range(1, 2).ToArray(), true);
                    return true;
                }
            case 5: //knight
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(7, 12).ToArray());
                    if (x.Count != 0)
                    {
                        FieldManager.instance.SetSelectablePanel(x.Select(i => i.model.thisFieldID).ToArray(), true);
                        return true;
                    }
                    break;
                }
        }
        return false;
    }
    /// <summary>
    /// 対象の確認 targetsByReceiverは対戦相手に送信される情報
    /// </summary>
    /// <param name="cc"></param>
    /// <param name="clickGameObject"></param>
    /// <returns></returns>
    private (bool passed, HeroController hctarget, CardController cctarget, int targetFieldIDByReceiver) TargetCheck(GameObject clickGameObject, TensionController tc)
    {

        HeroController hc = null;
        CardController c = null;
        clickGameObject?.TryGetComponent<HeroController>(out hc); //取れたらいいですね
        clickGameObject?.TryGetComponent<CardController>(out c); //取れたらいいですね

        switch (tc.model.tensionID)
        {
            case 2:
                {
                    if (c != null)
                    {
                        return (true, hc, c, FieldManager.instance.ChangeFieldID(c.model.thisFieldID));
                    }
                    if (hc != null)
                    {
                        return (true, hc, c, hc.model.isPlayer ? 14 : 13); //13は味方ヒーロー 14は敵ヒーローとする 受信者から見た対象なので、ここで敵味方の番号を入れ替えておく
                    }
                    return (false, null, null, 0);
                }
            case 5:
                {
                    if (c != null && c.model.isPlayerCard != tc.model.isPlayer)
                    {
                        return (true, hc, c, FieldManager.instance.ChangeFieldID(c.model.thisFieldID));
                    }
                    return (false, null, null, 0);
                }
            default:
                {
                    return (false, null, null, 0);
                }
        }
    }
}
