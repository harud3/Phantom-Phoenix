using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// スペルの発動前を管理する　CardプレハブとHeroブレハブとフィールドについてる
/// </summary>
public class DropSpellField : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!GameManager.instance.isPlayerTurn) { return; } //自分のターンではないのに置こうとしないで

        var targetC = GetComponent<CardController>() ?? null; //あるといいね
        var targetH = GetComponent<HeroController>() ?? null; //あるといいね

        if(targetC != null && !targetC.model.isFieldCard) { return; }　//手札にあるカードを対象にしてはいけない
        if (eventData.pointerDrag.GetComponent<CardController>() is var spell)
        {
            //MP超過のスペルやスペル以外は通さない
            if (spell == null || spell.model.cost > GameManager.instance.GetHeroMP(spell.model.isPlayerCard) || spell.model.category != CardEntity.Category.spell) { return; }

            var handIndex = spell.GetComponent<CardMovement>().siblingIndex; //対戦相手に手札のどのカードを使うかを送信するので

            //ターゲット情報を送る　ExecuteSpellContentsの方で、対象区分を基に分類を行い実行可否を決める  そのため、 とりあえず対象を送っておけばいい
            if (targetC != null)
            {
                //とりあえず、CardControllerを渡しておく
                if (spell.ExecuteSpellContents(targetC) && GameDataManager.instance.isOnlineBattle)
                {
                    SendExecuteSpellContents(handIndex, targetC.model.thisFieldID);
                }
            }
            else if (targetH != null)
            {
                //とりあえず、HeroControllerを渡しておく
                if (spell.ExecuteSpellContents(targetH) && GameDataManager.instance.isOnlineBattle)
                {
                    SendExecuteSpellContents(handIndex, targetH.model.isPlayer ? 13 : 14);
                }
            }
            else
            {
                //どうしようもないのでnullを渡しておく
                if (spell.ExecuteSpellContents<Controller>(null) && GameDataManager.instance.isOnlineBattle)
                {
                    SendExecuteSpellContents(handIndex);
                }
            }

        }
    }

    private void SendExecuteSpellContents(int handIndex, int targetID = 0)
    {
        //targetIDは味方目線なので、敵目線に変換しておく
        if(1 <= targetID && targetID <= 12) { targetID = FieldManager.instance.ChangeFieldID(targetID); }
        targetID = targetID == 13 ? 14 : targetID == 14 ? 13 : targetID; //13→14 14→13 他はそのまま
        GameManager.instance.SendExecuteSpellContents(handIndex, targetID);
    }
}
