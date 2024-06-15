using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class CardController : MonoBehaviour
{
    CardView view;
    public CardModel model {  get; private set; }
    public CardMovement movement {  get; private set; }
    private void Awake()
    {
        view = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
    }

    public void Init(int CardID, bool isPlayer)
    {
        model = new CardModel(CardID, isPlayer);
        view.Show(model);
    }
    public void putOnField(bool isPlayerField)
    {
        GameManager.instance.ReduceMP(model.cost, isPlayerField);
        model.SetIsFieldCard(true);
        view.HideCost(false);

        if (model.skill1 == CardEntity.skill.���� || model.skill2 == CardEntity.skill.���� || model.skill3 == CardEntity.skill.����)
        {
            SetCanAttack(true);
        }
        if (model.skill1 == CardEntity.skill.���� || model.skill2 == CardEntity.skill.���� || model.skill3 == CardEntity.skill.����)
        {
            if(!isPlayerField && (model.fieldID == 7 || model.fieldID == 8 || model.fieldID == 9)
                || (isPlayerField && (model.fieldID == 1 || model.fieldID == 2 || model.fieldID == 3))
                )
            {
                model.SetIs����(true);
                view.SetViewFrame����(true);
            }
        }
    }
    /// <summary>
    /// fieldID��1�`12 player1�`6 enemy7�`12
    /// </summary>
    /// <param name="fieldID"></param>
    public void MoveField(int fieldID)
    {
        model.SetIsFieldID(fieldID);
    }
    public void Attack(CardController enemyCard)
    {
        model.Attack(enemyCard);
        SetCanAttack(false);
    }
    public void SetCanAttack(bool canAttack)
    {
        model.SetCanAttack(canAttack);
        view.SetActiveSelectablePanel(canAttack);
    }
    public void CheckAlive()
    {
        if (model.isAlive) {
            view.ReShow(model);
        }
        else
        {
            Destroy(this.gameObject);
            
        }
    }
}
