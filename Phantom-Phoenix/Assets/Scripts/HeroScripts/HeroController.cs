using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : Controller
{
    HeroView view;
    public HeroModel model {  get; private set; }
    [SerializeField]
    private bool isPlayer; //player��enemy���@���O�ɃC���X�y�N�^�[��Ŏw�肵�Ă���
    private void Start()
    {
        view = GetComponent<HeroView>();
    }
    /// <summary>
    /// ����������
    /// </summary>
    /// <param name="heroID"></param>
    public void Init(int heroID)
    {
        model = new HeroModel(heroID, isPlayer);
        view.SetHero(model);
    }
    /// <summary>
    /// HP�̍ĕ\��
    /// </summary>
    private void ReShowHP()
    {
        view.ReShowHP(model);
    }
    /// <summary>
    /// �f�b�L���̍ĕ\��
    /// </summary>
    public void ReShowStackCards(int deckNum)
    {
        view.ReShowStackCards(deckNum);
    }
    /// <summary>
    /// �q�[���[���U���ɂ��_���[�W���󂯂����̏���
    /// </summary>
    public void DamageFromAttack(int dmg)
    {
        model.Damage(dmg);
        ReShowHP();
    }
    /// <summary>
    /// �q�[���[���X�y���ɂ��_���[�W���󂯂����̏��� 
    /// </summary>
    public void DamageFromSpell(int dmg)
    {
        AudioManager.instance.SoundCardFire();
        model.Damage(dmg + model.plusSpellDamage);
        ReShowHP();
        GameManager.instance.CheckIsAlive(model.isPlayer);
    }
    /// <summary>
    /// �q�[���[���_���[�W���󂯂����̏��� 
    /// </summary>
    public void Damage(int dmg)
    {
        AudioManager.instance.SoundCardFire();
        model.Damage(dmg);
        ReShowHP();
        GameManager.instance.CheckIsAlive(model.isPlayer);
    }
    public void Heal(int hl)
    {
        if (model.hp == model.maxHP) { return; }
        AudioManager.instance.SoundCardHeal();
        model.Heal(hl);
        ReShowHP();
    }
    /// <summary>
    /// ������߂���
    /// </summary>
    public void Concede()
    {
        model.Concede();
    }
    /// <summary>
    /// �^�[���J�n����MP���Z�b�g
    /// </summary>
    public void ResetMP()
    {
        model.ResetMP();
        view.ReShowMP(model);
        
    }
    /// <summary>
    /// (��ɃJ�[�h���o��������)MP�����炷
    /// </summary>
    /// <param name="reduce"></param>
    public void ReduceMP(int reduce)
    {
        model.ReduceMP(reduce);
        view.ReShowMP(model);
    }
    /// <summary>
    /// ���炩�̌��ʂɂ��MaxMP����������Ƃ��̏���
    /// </summary>
    /// <param name="up"></param>
    public void ChangeMaxMP(int up)
    {
        //�^�[���J�n���̍ő�MP�����́Amodel.ResetMP()�̒��ōs���Ă���̂ŁA���ʉ��͖�Ȃ�
        if (up > 0) { } 
        else { AudioManager.instance.SoundMPDeBuff(); }
        model.ChangeMaxMP(up);
        view.ReShowMP(model);
    }
    public void spellDamageBuff(int buff)
    {
        model.spellDamageBuff(buff);
    }
    public Action SpellUsedSkill = null;�@//�X�y���g�p�ɂ���Ĕ�������󓮓I�ȃX�L��
    public void ExecuteSpellUsedSkill()
    {
        SpellUsedSkill?.Invoke();
    }
    public Action<CardController> ccExternalBuff = new Action<CardController>((unit) => { });
}
/// <summary>
/// �J�[�h�ƃq�[���[�𓯎��ɏ����ł���悤�ɂ��邽�߂̊��N���X
/// </summary>
public class Controller : MonoBehaviour { }