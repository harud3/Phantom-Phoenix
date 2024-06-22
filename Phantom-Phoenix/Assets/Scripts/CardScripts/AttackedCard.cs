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
                StartCoroutine(ExecuteCardsBattle(attacker, target));
                
            }

            
        }
    }
    IEnumerator  ExecuteCardsBattle(CardController attacker, CardController target)
    {
        yield return new WaitForSeconds(0.25f);
        //開戦の儀
        AudioManager.instance.SoundCardAttack();
        GameManager.instance.CardsBattle(attacker, target);
        SkillManager.instance.ExecutePierce(attacker, target);
    }
}
