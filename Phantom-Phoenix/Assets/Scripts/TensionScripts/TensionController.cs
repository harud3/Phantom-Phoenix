using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CardEntity;

public class TensionController : MonoBehaviour
{
    TensionView view;
    [NonSerialized]
    public TensionModel model;
    [SerializeField]
    private bool isPlayer; //playerかenemyか　事前にインスペクター上で指定しておく
    [SerializeField] CardController cardPrefab;
    [SerializeField] HeroController playerHeroController, enemyHeroController;
    public void Init(int useHeroID)
    {
        view = GetComponent<TensionView>();
        model = new TensionModel(isPlayer, useHeroID);
        view.ReShow(model);

        SetTensionSpell(useHeroID);
    }
    /// <summary>
    /// テンションカードまたはテンションを使える表示の切替
    /// </summary>
    public void SetCanUseTension(bool canUseTension)
    {
        view.SetActiveSelectablePanel(canUseTension);
    }
    /// <summary>
    /// テンションカードを使う
    /// </summary>
    public void UseTensionCard()
    {
        if (GameManager.instance.GetHeroMP(isPlayer) <= 0) { return; }
        AudioManager.instance.SoundcTensionUp();
        model.UseTensionCard();
        view.ReShow(model);
    }
    /// <summary>
    /// テンションの値を設定する　hasSoundがtrueなら増減音を鳴らす
    /// </summary>
    public void SetTension(int tension, bool hasSound = true)
    {
        if(0 <= tension && tension <= 3)
        {
            if(tension > model.tension && hasSound)
            {
                AudioManager.instance.SoundcTensionUp();
            }
            else if(tension < model.tension && hasSound)
            {
                 AudioManager.instance.SoundcTensionDown();
            }
            model.SetTension(tension);
            view.ReShow(model);
        }
        if(GameManager.instance.gameState != GameManager.eGameState.isStarted) { return; }
        GameManager.instance.SetCanUsetension(model.isPlayer);
    }
    /// <summary>
    /// テンションカードを使うことができるかを設定　テンションを使った時に呼ばれるため、表示を変える
    /// </summary>
    public void CanUsetensionCard(bool canUseTensionCard)
    {
        model.CanUseTensionCard(canUseTensionCard);
        view.ReShow(model);
    }
    #region テンションスペルの実体
    //スペルカードと似ている
    /// <summary>
    /// ユニット単体を対象としたスペルの効果を設定
    /// </summary>
    public Action<CardController> ccSpellContents = new Action<CardController>((target) => { });
    /// <summary>
    /// ヒーロー単体を対象としたスペルの効果を設定
    /// </summary>
    public Action<HeroController> hcSpellContents = new Action<HeroController>((target) => { });
    /// <summary>
    /// 非選択効果のスペルの効果を設定
    /// </summary>
    public Action spellContents = new Action(() => { });
    public void UseTensionSpell<T>(T target) where T : Controller
    {
        CardController tc = target as CardController; //変換できたらいいですね
        HeroController th = target as HeroController; //変換できたらいいですね
        if (tc != null)
        {
            ccSpellContents?.Invoke(tc);
        }
        else if (th != null)
        {
            hcSpellContents?.Invoke(th);
        }
        else {
            spellContents?.Invoke();
        }
        SetTension(0, false); model.PlusTensionSpellUsedCnt();
        GameManager.instance.SetCanUsetension(model.isPlayer);
    }
    /// <summary>
    /// 各キャラのテンションスペルを設定
    /// </summary>
    private void SetTensionSpell(int useHeroID)
    {
        switch (useHeroID)
        {
            case 1: //エルフ
                void Summon011()
                {
                    if (FieldManager.instance.GetEmptyFieldID(model.isPlayer) is var x && x.emptyField != null)
                    {
                        CardController cc = Instantiate(cardPrefab, x.emptyField);
                        cc.Init(10001, model.isPlayer); // cardID10001 = token222;
                        cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                    }
                }
                spellContents = () =>
                {
                    Summon011();
                };
                break;
            case 2: //ウィッチ
                ccSpellContents = (CardController target) =>
                {
                    if (target.model.isPlayerCard == model.isPlayer)
                    {
                        target.Heal(3);
                    }
                    else
                    {
                        target.Damage(2);
                    }
                };
                hcSpellContents = (HeroController target) =>
                {
                    if (target.model.isPlayer == model.isPlayer)
                    {
                        target.Heal(3);
                    }
                    else
                    {
                        target.Damage(2);
                        GameManager.instance.CheckIsAlive(target.model.isPlayer);
                    }
                };
                break;
            case 3: //キング
                spellContents = () =>
                {
                    var target = FieldManager.instance.GetUnitsByIsPlayer(model.isPlayer);
                    if (target.Count == 1)
                    {
                        target.First().Buff(2, 2);
                    }
                    else
                    {
                        target.ForEach(i => i.Buff(1, 1));
                    }
                };
                break;
            case 4: //デーモン
                spellContents = () =>
                {
                    var target = FieldManager.instance.GetUnitsByIsPlayer(!model.isPlayer);
                    target.ForEach(i => i.DeBuff(1, 1));
                    if (FieldManager.instance.GetCardsInHand(!model.isPlayer) is var hc && hc.Count > 0)
                    {
                        hc.Last().CreaseCost(1);
                    }
                };
                break;
        }
        
    }
    #endregion
}
