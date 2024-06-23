using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

//攻撃対象側のカード カードについている
public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        //攻撃側が攻撃可能で、playerとenemyの関係なら
        if(eventData.pointerDrag.GetComponent<CardController>() is var attacker
            && GetComponent<CardController>() is var target
            && attacker.model.canAttack
            && attacker.model.isPlayerCard != target.model.isPlayerCard)
        {
            if (FieldManager.instance.CheckCanAttackUnit(attacker, target))
            {
                if (GameDataManager.instance.isOnlineBattle)
                {
                    GameManager.instance.SendCardBattle(attacker.model.thisFieldID, target.model.thisFieldID); //バトル情報を対戦相手に送信
                }
                ExecuteCardsBattle(attacker, target);
                
            }

            
        }
    }
    void  ExecuteCardsBattle(CardController attacker, CardController target)
    {
        AudioManager.instance.SoundCardAttack();
        //開戦の儀
        StartCoroutine(GameManager.instance.CardsBattle(attacker, target));
    }
}
