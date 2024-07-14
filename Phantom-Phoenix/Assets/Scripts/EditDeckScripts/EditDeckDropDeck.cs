using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

/// <summary>
/// デッキ編成画面　カードをデッキ欄にドロップした時の処理
/// </summary>
public class EditDeckDropDeck : MonoBehaviour, IDropHandler
{
    [SerializeField]
    TextMeshProUGUI hintText;
    public void OnDrop(PointerEventData eventData)
    {

        EditDeckCardController cc = eventData.pointerDrag.GetComponent<EditDeckCardController>();
        if (cc == null) { return; }

        if (this.transform.childCount < 30)
        {
            var rarity = cc.model.rarity;
            var cardID = cc.model.cardID;
            int maxCount = rarity == CardEntity.Rarity.SSR ? 1 : 3 ; //レア度で上限枚数が変わる
            var cardcount = 1;
#if UNITY_EDITOR
            maxCount = 30; //テスト用
#endif
            foreach (Transform t in this.transform)
            {
                if (t.gameObject.GetComponent<EditDeckCardController>().model.cardID == cardID) { cardcount++; }

                if (cardcount > maxCount) //欲張りだよそれは
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeText1());
                    return;
                }
            }

            //カードをデッキの子にする
            cc.movement.SetDefaultParent(this.transform);
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(ChangeText2());
            return;
        }
    }
    IEnumerator ChangeText1()
    {
        hintText.text = "R/SRは3枚　SSR は1枚 まで";
        yield return new WaitForSeconds(2f);
        hintText.text = "カードをデッキにドラッグ&ドロップ";
        StopAllCoroutines();
    }
    IEnumerator ChangeText2()
    {
        hintText.text = "デッキは30枚です";
        yield return new WaitForSeconds(2f);
        hintText.text = "カードをデッキにドラッグ&ドロップ";
        StopAllCoroutines();
    }
}
