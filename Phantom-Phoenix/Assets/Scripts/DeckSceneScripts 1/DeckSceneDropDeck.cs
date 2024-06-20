using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

/// <summary>
/// カードをfieldにドロップした時の処理
/// </summary>
public class DeckSceneDropDeck : MonoBehaviour, IDropHandler
{
    [SerializeField]
    TextMeshProUGUI hintText;
    public void OnDrop(PointerEventData eventData)
    {

        DeckSceneCardController cc = eventData.pointerDrag.GetComponent<DeckSceneCardController>();
        if (cc == null) { return; }

        if (this.transform.childCount < 30)
        {
            var rarity = cc.model.rarity;
            var cardID = cc.model.cardID;
            int maxCount = rarity == CardEntity.Rarity.SSR ? 1 : 3 ;
            var cardcount = 1;
#if UNITY_EDITOR
            maxCount = 30;
#endif
            foreach (Transform t in this.transform)
            {
                if (t.gameObject.GetComponent<DeckSceneCardController>().model.cardID == cardID) { cardcount++; }

                if (cardcount > maxCount)
                {
                    StopAllCoroutines();
                    StartCoroutine(ChangeText());
                    return;
                }
            }
            //カードをfieldに置く処理
            cc.movement.SetDefaultParent(this.transform);
        }
    }
    IEnumerator ChangeText()
    {
        hintText.text = "R/SRは3枚　SSR は1枚 まで";
        yield return new WaitForSeconds(2f);
        hintText.text = "カードをデッキにドラッグ&ドロップ";
        StopAllCoroutines();
    }
}
