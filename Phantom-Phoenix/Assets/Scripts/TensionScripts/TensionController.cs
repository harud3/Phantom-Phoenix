using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardEntity;

public class TensionController : MonoBehaviour
{
    TensionView view;
    [NonSerialized]
    public TensionModel model;
    [SerializeField]
    private bool isPlayer; //player��enemy���@���O�ɃC���X�y�N�^�[��Ŏw�肵�Ă���
    private void Start()
    {
        view = GetComponent<TensionView>();
        model = new TensionModel(isPlayer);
        view.ReShow(model);

        ccSpellContents = (CardController target) => {
            if (target.model.isPlayerCard == model.isPlayer)
            {
                target.Heal(3);
            }
            else
            {
                target.Damage(2);
            }
        };
        hcSpellContents = (HeroController target) => {
            if (target.model.isPlayer == model.isPlayer)
            {
                target.Heal(3);
            }
            else
            {
                target.Damage(2);
            }
        };
    }
    public void SetCanUseTension(bool canUseTension)
    {
        view.SetActiveSelectablePanel(canUseTension);
    }
    public void UseTensionCard()
    {
        if (GameManager.instance.GetHeroMP(isPlayer) <= 0) { return; }
        AudioManager.instance.SoundcUseTensionCard();
        model.UseTensionCard();
        view.ReShow(model);
    }
    public void SetTension(int tension)
    {
        if(0 <= tension && tension <= 3)
        {
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
        SetTension(0); model.PlusTensionSpellUsedCnt();
        GameManager.instance.SetCanUsetension(model.isPlayer);
    }
}
