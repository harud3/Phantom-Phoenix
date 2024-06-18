using System.Collections;
using System.Collections.Generic;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpellDropManager : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var targetC = GetComponent<CardController>() ?? null;
        var targetH = GetComponent<HeroController>() ?? null;

        if(targetC != null && !targetC.model.isFieldCard) { return; }
        if (eventData.pointerDrag.GetComponent<CardController>() is var spell)
        {
            if (spell == null || spell.model.category != CardEntity.Category.spell) { return; }
            if (targetC != null)
            {
                spell.ExecuteSpellContents(targetC);
            }
            else if (targetH != null)
            {
                spell.ExecuteSpellContents(targetH);
            }
            else
            {
                spell.ExecuteSpellContents<Controller>(null);
            }

        }
    }
}
