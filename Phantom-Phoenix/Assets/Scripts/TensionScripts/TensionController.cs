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
    private bool isPlayer; //player��enemy���@���O�ɃC���X�y�N�^�[��Ŏw�肵�Ă���
    [SerializeField] CardController cardPrefab;
    [SerializeField] HeroController playerHeroController, enemyHeroController;
    public void Init(int useHeroID)
    {
        view = GetComponent<TensionView>();
        model = new TensionModel(isPlayer, useHeroID);
        view.ReShow(model);

        SetTensionSpell(useHeroID);
    }
    public void SetCanUseTension(bool canUseTension)
    {
        view.SetActiveSelectablePanel(canUseTension);
    }
    public void UseTensionCard()
    {
        if (GameManager.instance.GetHeroMP(isPlayer) <= 0) { return; }
        AudioManager.instance.SoundcTensionUp();
        model.UseTensionCard();
        view.ReShow(model);
    }
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
    public void CanUsetensionCard(bool canUseTensionCard)
    {
        model.CanUseTensionCard(canUseTensionCard);
        view.ReShow(model);
    }
    /// <summary>
    /// ���j�b�g�P�̂�ΏۂƂ����X�y���̌��ʂ�ݒ�
    /// </summary>
    public Action<CardController> ccSpellContents = new Action<CardController>((target) => { });
    /// <summary>
    /// �q�[���[�P�̂�ΏۂƂ����X�y���̌��ʂ�ݒ�
    /// </summary>
    public Action<HeroController> hcSpellContents = new Action<HeroController>((target) => { });
    /// <summary>
    /// ��I�����ʂ̃X�y���̌��ʂ�ݒ�
    /// </summary>
    public Action spellContents = new Action(() => { });
    public void UseTensionSpell<T>(T target) where T : Controller
    {
        CardController tc = target as CardController; //�ϊ��ł����炢���ł���
        HeroController th = target as HeroController; //�ϊ��ł����炢���ł���
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
    private void SetTensionSpell(int useHeroID)
    {
        switch (useHeroID)
        {
            case 1: //elf
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
            case 2: //witch
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
            case 3: //king
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
            case 4: //demon
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
            case 5: //knight
                var x = model.isPlayer ? playerHeroController : enemyHeroController;
                ccSpellContents = (CardController target) =>
                {
                    target.Damage(1 + ((x.model.maxHP - x.model.hp) / 5));
                    SkillManager.instance.ExecutePierce(1 + ((x.model.maxHP - x.model.hp) / 5), target);
                };
                break;
        }
        
    }
}
