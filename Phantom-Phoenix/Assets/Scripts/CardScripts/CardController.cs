using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class CardController : Controller
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

        SkillManager.instance.specialSkills(this);

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
    public void CheckAlive()
    {
        if (model.isAlive)
        {
            view.ReShow(model);
        }
        else
        {
            Destroy(this.gameObject);

        }
    }
    public void Attack<T>(T enemy, bool isAttacker) where T : Controller
    {
        ExecuteSpecialSkillBeforeAttack(isAttacker);
        model.Attack(enemy);
        ExecuteSpecialSkillAfterAttack(isAttacker);
        if (SkillManager.instance.isActiveDoubleAction(model))
        {
            SetCanAttack(true, false);
            model.SetisActiveDoubleAction(false);
        }
        else { SetCanAttack(false, false); }
    }
    
    public Action<bool> SpecialSkillBeforeAttack = new Action<bool>((isAttacker) => { });
    public void ExecuteSpecialSkillBeforeAttack(bool isAttacker)
    {
        SpecialSkillBeforeAttack(isAttacker);
    }
    public Action<bool> SpecialSkillAfterAttack = new Action<bool>((isAttacker) => { });
    public void ExecuteSpecialSkillAfterAttack(bool isAttacker)
    {
        SpecialSkillAfterAttack(isAttacker);
    }
    public void SetCanAttack(bool canAttack, bool ResetIsActiveDoubleAction)
    {
        model.SetCanAttack(canAttack);
        view.SetActiveSelectablePanel(canAttack);
        if(ResetIsActiveDoubleAction && SkillManager.instance.isDoubleAction(model)) {
            model.SetisActiveDoubleAction(true);
        }
    }
}
