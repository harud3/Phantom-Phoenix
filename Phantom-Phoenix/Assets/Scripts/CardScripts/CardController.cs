using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering.Universal;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// カード関連処理の統括　Cardプレハブについてる
/// </summary>
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
    public Action UpdateSkill = null;　//外部要因によって発生する受動的なスキル

    public void Init(int CardID, bool isPlayer = true)
    {
        model = new CardModel(CardID, isPlayer);
        if (isPlayer)
        {
            SkillManager.instance.playerHeroController.ccExternalBuff(this);
        }
        else
        {
            SkillManager.instance.enemyHeroController.ccExternalBuff(this);
        }
        view.SetCard(model);
        SkillManager.instance.UpdateSkills(this);

        //スペルカードならここで効果を設定しておく　
        if(model.category == CardEntity.Category.spell)
        {
            SkillManager.instance.SpecialSkills(this);
        }
    }
    /// <summary>
    /// ユニット単体を対象としたスペルの効果を設定
    /// </summary>
    public Action<CardController> ccSpellContents = new Action<CardController>((target) => { });
    /// <summary>
    /// ヒーロー単体を対象としたスペルの効果を設定
    /// </summary>
    public Action<HeroController> hcSpellContents = new Action<HeroController>((target) => { });
    /// <summary>
    /// スペルの効果を設定
    /// </summary>
    public Action SpellContents = new Action(() => { });
    /// <summary>
    /// 前述のスペル効果を発動する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    public void ExecuteSpellContents<T>(T target)where T : Controller
    {
        CardController tc = target as CardController; //変換できたらいいですね
        HeroController th = target as HeroController; //変換できたらいいですね

        void Execute(Action ac)
        {
            //発動するなら、自身を破壊する　スペルカードがフィールドに出たらおかしいので
            GameManager.instance.ReduceMP(model.cost, model.isPlayerCard);
            ac();
            Destroy(this.gameObject); 
        }

        //効果範囲に合わせて処理が変わる
        switch (model.target)
        {
            case CardEntity.Target.none: //何もしない
                return;
            case CardEntity.Target.unit: //tcがあればヨシ
            case CardEntity.Target.selectionArea: //対象範囲に含まれたtcがないと、どの範囲なのか識別できない
                if (tc != null) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.Target.enemyUnit: //tcの敵対チェック
            case CardEntity.Target.selectionEnemyArea: //tcの敵対チェック 　対象範囲に含まれたtcがないと、どの範囲なのか識別できない
                if (tc != null && tc.model.isPlayerCard != model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.Target.playerUnit: //tcの友好チェック
            case CardEntity.Target.selectionPlayerArea:　//tcの友好チェック 　対象範囲に含まれたtcがないと、どの範囲なのか識別できない
                if (tc != null && tc.model.isPlayerCard == model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.Target.hero: //thがあればヨシ
                if (th != null) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.unitOrHero: //対象があればヨシ
                if (tc != null) { Execute(() => ccSpellContents(tc)); }
                else if (th != null) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.enemy: //tcの存在 & 敵対　thの存在 & 敵対　が必要
                if (tc != null && tc.model.isPlayerCard != model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                else if (th != null && th.model.isPlayer != model.isPlayerCard) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.player:　//tcの存在 & 友好　thの存在 & 友好　が必要
                if (tc != null && tc.model.isPlayerCard == model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                else if (th != null && th.model.isPlayer == model.isPlayerCard) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.area: //効果範囲が決まってるスペル
                Execute(() => SpellContents());
                return;
        }
    }
    /// <summary>
    /// フィールドに召喚する時の処理　　まれに、召喚時効果で対象選択を必要とする場合がある
    /// </summary>
    /// <param name="targets"></param>
    public void SummonOnField(int fieldID, CardController[] targets = null, bool ExecuteReduceMP = true)
    {
        AudioManager.instance.SoundCardMove();

        if (ExecuteReduceMP) { GameManager.instance.ReduceMP(model.cost, model.isPlayerCard); } //ヒーローのMPを減らす
        model.SetIsFieldCard(true);
        model.SetThisFieldID(fieldID);
        view.HideCost(false);
        Show(true);

        FieldManager.instance.SetFieldOnUnitcnt(model.isPlayerCard); //ユニット配置数の再設定
        SkillManager.instance.SpecialSkills(this, targets); //召喚時効果の発動　誘発効果の紐づけ

        SetCanAttack(SkillManager.instance.IsFast(model)); //即撃付与 CanSummonの無効化も兼ねる

        if (SkillManager.instance.IsTaunt(model)) //挑発効果付与 前列に召喚された時だけ
        {
            model.SetIsTaunt(true);
            view.SetViewFrameTaunt(true);
        }
        if (SkillManager.instance.IsSnipe(model)) //狙撃効果付与
        {
            view.SetViewFrameSnipe(true);
        }
        if (SkillManager.instance.IsPierce(model)) //貫通効果付与
        {
            view.SetViewFramePierce(true);
        }
        if (SkillManager.instance.IsDoubleAction(model))
        { //連撃効果・連撃権付与
            model.SetIsActiveDoubleAction(true);
            view.SetViewFrameDoubleAction(true);
        }
    }
    /// <summary>
    /// 表面・裏面の表示切替
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void Show(bool viewOpenSide)
    {
        view.Show(viewOpenSide);
    }
    /// <summary>
    /// マリガン候補かどうか
    /// </summary>
    public void SetIsMulliganCard()
    {
        model.SetIsMulliganCard();
        view.SetActiveSelectablePanel(true); //最初は返さない前提とする
    }
    /// <summary>
    /// マリガンするかどうか
    /// </summary>
    /// <param name="isMulligan"></param>
    public void SetIsMulligan(bool isMulligan)
    {
        model.SetIsMulligan(isMulligan);
        view.SetActiveSelectablePanel(!isMulligan); //マリガンで返す→光らせない　マリガンで返さない→光らせる　なので否定する
    }
    /// <summary>
    /// コストを指定された値にする
    /// </summary>
    /// <param name="nextCost"></param>
    public void ChangeCost(int nextCost)
    {
        model.ChangeCost(nextCost);
        view.ReShow(model);
    }
    /// <summary>
    /// ユニットが攻撃によりダメージを受けた時の処理　ここでは、CheckAliveは不都合が出るので行わない
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void DamageFromAttack(int dmg) {
        model.Damage(dmg);
        view.ReShow(model);
    }
    /// <summary>
    /// ユニットがダメージを受けた時の処理 
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void Damage(int dmg)
    {
        AudioManager.instance.SoundCardFire();
        model.Damage(dmg);
        view.ReShow(model);
        StartCoroutine(CheckAlive());
    }
    /// <summary>
    /// ユニットが回復を受けた時の処理
    /// </summary>
    /// <param name="hl"></param>
    public void Heal(int hl)
    {
        if (model.hp == model.maxHP) { return; }
        AudioManager.instance.SoundCardHeal();
        model.Heal(hl);
        view.ReShow(model);
    }
    /// <summary>
    /// 生きているかどうかの判定　生きていないなら破壊する
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckAlive()
    {
        if (model.isAlive)
        {
            view.ReShow(model);
        }
        else
        {
            yield return null; //他の処理待ち
            if (!ExecutedSSBD) //死亡時効果が2回発動していたので対策　原因不明
            {
                ExecuteSpecialSkillBeforeDie(); //死亡時効果
                ExecutedSSBD = true;
            }
            FieldManager.instance.Minus1FieldOnUnitCnt(model.isPlayerCard);
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// ユニットかヒーローに攻撃する
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enemy"></param>
    /// <param name="isAttacker"></param>
    public void Attack<T>(T enemy, bool isAttacker) where T : Controller
    {
        ExecuteSpecialSkillBeforeAttack(isAttacker); //攻撃前効果 攻撃時はこちら
        model.Attack(enemy);
        ExecuteSpecialSkillAfterAttack(isAttacker); //攻撃後効果
        if (!model.isAlive) { return; }　//しんでるなら連撃は考えない

        //連撃判定の特殊処理
        if (SkillManager.instance.IsActiveDoubleAction(model)) //ユニットが連撃持ちで、連撃権があるならtrue
        {
            SetCanAttack(true); //もう1回戦えるドン
            model.SetIsActiveDoubleAction(false); //連撃権の無効化
        }
        else { SetCanAttack(false); }
    }
    /// <summary>
    /// 攻撃可能にする　連撃権の復活もこなす ターン開始時に呼ぶ時に連撃権が復活する想定
    /// </summary>
    /// <param name="canAttack"></param>
    /// <param name="ResetIsActiveDoubleAction"></param>
    public void SetCanAttack(bool canAttack, bool ResetIsActiveDoubleAction = false)
    {
        model.SetCanAttack(canAttack);
        view.SetActiveSelectablePanel(canAttack);
        if (ResetIsActiveDoubleAction && SkillManager.instance.IsDoubleAction(model))
        {
            model.SetIsActiveDoubleAction(true);
        }
    }
    public void SetIsNotSummonThisTurn()
    {
        model.SetIsNotSummonThisTurn();
    }
    public void SetCanSummon(bool canSummon)
    {
        view.SetActiveSelectablePanel(canSummon);
    }
    /// <summary>
    /// 攻撃前効果　攻撃時効果
    /// </summary>
    public Action<bool> SpecialSkillBeforeAttack = null;
    public void ExecuteSpecialSkillBeforeAttack(bool isAttacker)
    {
        if(!model.isSeal)SpecialSkillBeforeAttack?.Invoke(isAttacker);
    }
    /// <summary>
    /// 攻撃後効果 使うことあるんだろうか…
    /// </summary>
    public Action<bool> SpecialSkillAfterAttack = null;
    public void ExecuteSpecialSkillAfterAttack(bool isAttacker)
    {
        if (!model.isSeal) SpecialSkillAfterAttack?.Invoke(isAttacker);
    }
    /// <summary>
    /// ターン終了時効果
    /// </summary>
    public Action<bool> SpecialSkillEndTurn = null;
    public void ExecuteSpecialSkillEndTurn(bool isPlayerTurn)
    {
        if (!model.isSeal) SpecialSkillEndTurn?.Invoke(isPlayerTurn);
    }
    private bool ExecutedSSBD = false;
    /// <summary>
    /// 死亡時効果
    /// </summary>
    public Action SpecialSkillBeforeDie = null;
    public void ExecuteSpecialSkillBeforeDie()
    {
        if (!model.isSeal) SpecialSkillBeforeDie?.Invoke();
    }
    public void SetIsBurning(bool isBurning)
    {
        view.SetViewFrameBurning(isBurning);
    }
    public void SetIsSnipe(bool isSnipe)
    {
        if (model.skill4 == CardEntity.Skill.none)
        {
            model.skill4 = CardEntity.Skill.snipe;
        }
        else if(model.skill5 == CardEntity.Skill.none)
        {
            model.skill5 = CardEntity.Skill.snipe;
        }
        view.SetViewFrameSnipe(isSnipe);
    }
    /// <summary>
    /// 封印効果
    /// </summary>
    /// <param name="isSeal"></param>
    public void SetIsSeal(bool isSeal)
    {
        model.SetIsSeal(isSeal);
        view.SetViewFrameSeal(isSeal);
        view.SetViewFrameTaunt(!isSeal);
        view.SetViewFrameSnipe(!isSeal);
        view.SetViewFrameDoubleAction(!isSeal);
        view.SetViewFramePierce(!isSeal);
        view.SetViewFrameBurning(!isSeal);
        if (model.isSummonThisTurn || SkillManager.instance.HasDoubleActionAndIsNotActiveDoubleAction(model))//即撃対策 連撃対策
        {
            SetCanAttack(false);
        }
        view.ReShow(model);
    }
    public void Buff(int atk, int hp)
    {
        AudioManager.instance.SoundcCardBuff();
        SilentBuff(atk, hp);
    }
    public void SilentBuff(int atk, int hp)
    {
        model.Buff(atk, hp);
        view.ReShow(model);
    }
    public void DeBuff(int atk, int hp)
    {
        AudioManager.instance.SoundcCardDeBuff();
        model.DeBuff(atk, hp);
        view.ReShow(model);
        StartCoroutine(CheckAlive());
    }
}
