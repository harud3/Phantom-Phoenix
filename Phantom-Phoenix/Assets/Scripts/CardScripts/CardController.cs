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
        GameManager.instance.ReduceMP(model.cost, model.isPlayerCard);
        model.SetIsFieldCard(true);
        view.HideCost(false);

        SkillManager.instance.specialSkills(model.cardID, model.isPlayerCard);

        if (SkillManager.instance.isFast(model))
        {
            SetCanAttack(true, false);
        }
        if (SkillManager.instance.isTaunt(model, model.isPlayerCard))
        {
            model.SetisTaunt(true);
            view.SetViewFrameTaunt(true);
        }
        if (SkillManager.instance.isDoubleAction(model)){
            model.SetisActiveDoubleAction(true);
        }

    }
    /// <summary>
    /// fieldIDÇÕ1Å`12 player1Å`6 enemy7Å`12
    /// </summary>
    /// <param name="fieldID"></param>
    public void MoveField(int fieldID)
    {
        model.SetIsFieldID(fieldID);
    }
    public void Damage(int dmg)
    {
        model.Damage(dmg);
        view.ReShow(model);
        CheckAlive();
    }
    public void Attack(CardController enemyCard)
    {
        model.Attack(enemyCard);
        if (SkillManager.instance.isActiveDoubleAction(model))
        {
            SetCanAttack(true, false);
            model.SetisActiveDoubleAction(false);
        }
        else { SetCanAttack(false, false); }
    }
    public void Attack(HeroController enemyHero)
    {
        enemyHero.Damage(model.atk);
        if (SkillManager.instance.isActiveDoubleAction(model))
        {
            SetCanAttack(true, false);
            model.SetisActiveDoubleAction(false);
        }
        else { SetCanAttack(false, false); }
    }
    public void SetCanAttack(bool canAttack, bool ResetIsActiveDoubleAction)
    {
        model.SetCanAttack(canAttack);
        view.SetActiveSelectablePanel(canAttack);
        if(ResetIsActiveDoubleAction && SkillManager.instance.isDoubleAction(model)) {
            model.SetisActiveDoubleAction(true);
        }
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
