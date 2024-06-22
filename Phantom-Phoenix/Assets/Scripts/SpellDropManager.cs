using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// スペルの発動前を管理する　CardプレハブとHeroブレハブとフィールドについてる
/// </summary>
public class SpellDropManager : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var targetC = GetComponent<CardController>() ?? null; //あるといいね
        var targetH = GetComponent<HeroController>() ?? null; //あるといいね

        if(targetC != null && !targetC.model.isFieldCard) { return; }　//手札にあるカードを対象にしてはいけない
        if (eventData.pointerDrag.GetComponent<CardController>() is var spell)
        {
            
            if (spell == null || spell.model.category != CardEntity.Category.spell) { return; } //スペル以外は通さない
            if (targetC != null)
            {
                spell.ExecuteSpellContents(targetC); //とりあえず、CardControllerを渡しておく
            }
            else if (targetH != null)
            {
                spell.ExecuteSpellContents(targetH); //とりあえず、HeroControllerを渡しておく
            }
            else
            {
                spell.ExecuteSpellContents<Controller>(null); //どうしようもないのでnullを渡しておく
            }

        }
    }
}
