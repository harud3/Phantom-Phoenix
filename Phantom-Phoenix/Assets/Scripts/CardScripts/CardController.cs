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
/// �J�[�h�֘A�����̓����@Card�v���n�u�ɂ��Ă�
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

    public void Init(int CardID, bool isPlayer = true)
    {
        model = new CardModel(CardID, isPlayer);
        view.SetCard(model);

        //�X�y���J�[�h�Ȃ炱���Ō��ʂ�ݒ肵�Ă����@
        if(model.category == CardEntity.Category.spell)
        {
            SkillManager.instance.specialSkills(this);
        }
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
    /// �X�y���̌��ʂ�ݒ�
    /// </summary>
    public Action SpellContents = new Action(() => { });
    /// <summary>
    /// �O�q�̃X�y�����ʂ𔭓�����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    public void ExecuteSpellContents<T>(T target)where T : Controller
    {
        CardController tc = target as CardController; //�ϊ��ł����炢���ł���
        HeroController th = target as HeroController; //�ϊ��ł����炢���ł���

        void Execute(Action ac)
        {
            //��������Ȃ�A���g��j�󂷂�@�X�y���J�[�h���t�B�[���h�ɏo���炨�������̂�
            GameManager.instance.ReduceMP(model.cost, model.isPlayerCard);
            ac();
            Destroy(this.gameObject); 
        }

        //���ʔ͈͂ɍ��킹�ď������ς��
        switch (model.target)
        {
            case CardEntity.Target.none: //�������Ȃ�
                return;
            case CardEntity.Target.unit: //tc������΃��V
            case CardEntity.Target.selectionArea: //�Ώ۔͈͂Ɋ܂܂ꂽtc���Ȃ��ƁA�ǂ͈̔͂Ȃ̂����ʂł��Ȃ�
                if (tc != null) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.Target.enemyUnit: //tc�̓G�΃`�F�b�N
            case CardEntity.Target.selectionEnemyArea: //tc�̓G�΃`�F�b�N �@�Ώ۔͈͂Ɋ܂܂ꂽtc���Ȃ��ƁA�ǂ͈̔͂Ȃ̂����ʂł��Ȃ�
                if (tc != null && tc.model.isPlayerCard != model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.Target.playerUnit: //tc�̗F�D�`�F�b�N
            case CardEntity.Target.selectionPlayerArea:�@//tc�̗F�D�`�F�b�N �@�Ώ۔͈͂Ɋ܂܂ꂽtc���Ȃ��ƁA�ǂ͈̔͂Ȃ̂����ʂł��Ȃ�
                if (tc != null && tc.model.isPlayerCard == model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                return;
            case CardEntity.Target.hero: //th������΃��V
                if (th != null) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.unitOrHero: //�Ώۂ�����΃��V
                if (tc != null) { Execute(() => ccSpellContents(tc)); }
                else if (th != null) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.enemy: //tc�̑��� & �G�΁@th�̑��� & �G�΁@���K�v
                if (tc != null && tc.model.isPlayerCard != model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                else if (th != null && th.model.isPlayer != model.isPlayerCard) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.player:�@//tc�̑��� & �F�D�@th�̑��� & �F�D�@���K�v
                if (tc != null && tc.model.isPlayerCard == model.isPlayerCard) { Execute(() => ccSpellContents(tc)); }
                else if (th != null && th.model.isPlayer == model.isPlayerCard) { Execute(() => hcSpellContents(th)); }
                return;
            case CardEntity.Target.area: //���ʔ͈͂����܂��Ă�X�y��
                Execute(() => SpellContents());
                return;
        }
    }
    /// <summary>
    /// �t�B�[���h�ɏ������鎞�̏����@�@�܂�ɁA���������ʂőΏۑI����K�v�Ƃ���ꍇ������
    /// </summary>
    /// <param name="isPlayerField"></param>
    /// <param name="targets"></param>
    public void SummonOnField(bool isPlayerField, int fieldID, CardController[] targets = null)
    {
        AudioManager.instance.SoundCardMove();

        GameManager.instance.ReduceMP(model.cost, model.isPlayerCard); //�q�[���[��MP�����炷�@isPlayerCard�ƁAisPlayer�͈�v���Ă���ɈႢ�Ȃ�
        model.SetIsFieldCard(true);
        model.SetThisFieldID(fieldID);
        view.HideCost(false);
        Show(true);

        SkillManager.instance.specialSkills(this, targets); //���������ʂ̔����@�U�����ʂ̕R�Â�

        SetCanAttack(SkillManager.instance.isFast(model)); //�����t�^ CanSummon�̖����������˂�

        if (SkillManager.instance.isTaunt(model)) //�������ʕt�^ �O��ɏ������ꂽ������
        {
            model.SetIsTaunt(true);
            view.SetViewFrameTaunt(true);
        }
        if (SkillManager.instance.isDoubleAction(model))
        { //���U�����ʕt�^
            model.SetIsActiveDoubleAction(true);
        }

    }
    /// <summary>
    /// �\�ʁE���ʂ̕\���ؑ�
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void Show(bool viewOpenSide)
    {
        view.Show(viewOpenSide);
    }
    /// <summary>
    /// �}���K����₩�ǂ���
    /// </summary>
    public void SetIsMulliganCard()
    {
        model.SetIsMulliganCard();
        view.SetActiveSelectablePanel(true); //�ŏ��͕Ԃ��Ȃ��O��Ƃ���
    }
    /// <summary>
    /// �}���K�����邩�ǂ���
    /// </summary>
    /// <param name="isMulligan"></param>
    public void SetIsMulligan(bool isMulligan)
    {
        model.SetIsMulligan(isMulligan);
        view.SetActiveSelectablePanel(!isMulligan); //�}���K���ŕԂ������点�Ȃ��@�}���K���ŕԂ��Ȃ������点��@�Ȃ̂Ŕے肷��
    }
    /// <summary>
    /// ���j�b�g���U���ɂ��_���[�W���󂯂����̏����@�����ł́ACheckAlive�͕s�s�����o��̂ōs��Ȃ�
    /// </summary>
    /// <param name="viewOpenSide"></param>
    public void DamageFromAttack(int dmg) {
        model.Damage(dmg);
        view.ReShow(model);
    }
    /// <summary>
    /// ���j�b�g���_���[�W���󂯂����̏��� 
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
    /// ���j�b�g���񕜂��󂯂����̏���
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
    /// �����Ă��邩�ǂ����̔���@�����Ă��Ȃ��Ȃ�j�󂷂�
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
            yield return null; //���̏����҂�
            if (!ExecutedSSBD) //���S�����ʂ�2�񔭓����Ă����̂ő΍�@�����s��
            {
                ExecuteSpecialSkillBeforeDie(); //���S������
                ExecutedSSBD = true;
            }
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// ���j�b�g���q�[���[�ɍU������
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enemy"></param>
    /// <param name="isAttacker"></param>
    public void Attack<T>(T enemy, bool isAttacker) where T : Controller
    {
        ExecuteSpecialSkillBeforeAttack(isAttacker); //�U���O���� �U�����͂�����
        model.Attack(enemy);
        ExecuteSpecialSkillAfterAttack(isAttacker); //�U�������
        if (!model.isAlive) { return; }�@//����ł�Ȃ�A���͍l���Ȃ�

        //�A������̓��ꏈ��
        if (SkillManager.instance.isActiveDoubleAction(model)) //���j�b�g���A�������ŁA�A����������Ȃ�true
        {
            SetCanAttack(true); //����1��킦��h��
            model.SetIsActiveDoubleAction(false); //�A�����̖�����
        }
        else { SetCanAttack(false); }
    }
    /// <summary>
    /// �U���\�ɂ���@�A�����̕��������Ȃ� �^�[���J�n���ɌĂԎ��ɘA��������������z��
    /// </summary>
    /// <param name="canAttack"></param>
    /// <param name="ResetIsActiveDoubleAction"></param>
    public void SetCanAttack(bool canAttack, bool ResetIsActiveDoubleAction = false)
    {
        model.SetCanAttack(canAttack);
        view.SetActiveSelectablePanel(canAttack);
        if (ResetIsActiveDoubleAction && SkillManager.instance.isDoubleAction(model))
        {
            model.SetIsActiveDoubleAction(true);
        }
    }
    public void SetCanSummon(bool canSummon)
    {
        view.SetActiveSelectablePanel(canSummon);
    }
    /// <summary>
    /// �U���O���ʁ@�U��������
    /// </summary>
    public Action<bool> SpecialSkillBeforeAttack = null;
    public void ExecuteSpecialSkillBeforeAttack(bool isAttacker)
    {
        SpecialSkillBeforeAttack?.Invoke(isAttacker);
    }
    /// <summary>
    /// �U������� �g�����Ƃ���񂾂낤���c
    /// </summary>
    public Action<bool> SpecialSkillAfterAttack = null;
    public void ExecuteSpecialSkillAfterAttack(bool isAttacker)
    {
        SpecialSkillAfterAttack?.Invoke(isAttacker);
    }
    /// <summary>
    /// �^�[���I��������
    /// </summary>
    public Action<bool> SpecialSkillEndTurn = null;
    public void ExecuteSpecialSkillEndTurn(bool isPlayerTurn)
    {
        SpecialSkillEndTurn?.Invoke(isPlayerTurn);
    }
    private bool ExecutedSSBD = false;
    /// <summary>
    /// ���S������
    /// </summary>
    public Action SpecialSkillBeforeDie = null;
    public void ExecuteSpecialSkillBeforeDie()
    {
        SpecialSkillBeforeDie?.Invoke();
    }
}
