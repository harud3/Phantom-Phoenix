using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField]
    bool isPlayerField;
    public void OnDrop(PointerEventData eventData)
    {
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        if (isPlayerField != card.model.isPlayerCard || card.model.isFieldCard)
        {
            return;
        }
        if (card != null) {
            
            if (card.movement.isDraggable && this.transform.childCount == 0)
            {
                card.movement.defaultParent = this.transform;
                if (!card.model.isFieldCard)
                {
                    GameManager.instance.ReduceMP(card.model.cost, true);
                    card.model.isFieldCard = true;
                }
            }
        }
    }
}
