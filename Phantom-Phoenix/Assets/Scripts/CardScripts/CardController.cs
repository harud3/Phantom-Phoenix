using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering.Universal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.EventSystems.EventTrigger;

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
        if(model.category == CardEntity.Category.spell)
        {
            SkillManager.instance.specialSkills(this);
        }
    }
    public Action<CardController> ccSpellContents = new Action<CardController>((target) => { });
    public Action<HeroController> hcSpellContents = new Action<HeroController>((target) => { });
    public Action SpellContents = new Action(() => { });
    public void ExecuteSpellContents<T>(T target)where T : Controller
    {
        CardController tc = target as CardController;
        HeroController th = target as HeroController;
        void Execute(Action ac)
        {
            GameManager.instance.ReduceMP(model.cost, model.isPlayerCard);
            ac();
            Destroy(this.gameObject); 
        }

        switch (model.spellTarget)
        {
            case CardEntity.SpellTarget.none: 
                return;
            case CardEntity.SpellTarget.unit:
            case CardEntity.SpellTarget.selectionArea:
                if (tc != null) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.SpellTarget.enemyUnit:
            case CardEntity.SpellTarget.selectionEnemyArea:
                if (tc != null && tc.model.isPlayerCard != model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.SpellTarget.playerUnit:
            case CardEntity.SpellTarget.selectionPlayerArea:
                if (tc != null && tc.model.isPlayerCard == model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.SpellTarget.hero:
                if(th != null) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.SpellTarget.unitOrHero:
                if (tc != null) { Execute(() => ccSpellContents(tc)); }
                else if (th != null) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.SpellTarget.enemy:
                if (tc != null && tc.model.isPlayerCard != model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                else if (th != null && th.model.isPlayer != model.isPlayerCard) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.SpellTarget.player:
                if (tc != null && tc.model.isPlayerCard == model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                else if (th != null && th.model.isPlayer == model.isPlayerCard) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.SpellTarget.area:
                Execute(() => SpellContents());
                return;
        }
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
        if (!model.isAlive) { return; }
        //òAåÇópÇÃì¡éÍèàóù
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
