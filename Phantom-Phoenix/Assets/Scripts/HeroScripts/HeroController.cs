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
    public void DamageFromSpell(int dmg, bool isPlayer)
    {
        AudioManager.instance.SoundCardFire();
        model.Damage(dmg + GameManager.instance.GetPlusSpellDamage(isPlayer));
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
    /// <summary>
    /// HP���񕜂���
    /// </summary>
    /// <param name="hl"></param>
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
    /// MP�𑝂₷
    /// </summary>
    public void IncreaseMP(int increase)
    {
        if(increase > 0)
        {
            AudioManager.instance.SoundMPHeal();
        }
        model.ReduceMP(-increase); //�������Ȃ��̂� �Ƃ͎v��
        view.ReShowMP(model);
        GameManager.instance.SetCanUsetension(model.isPlayer);
    }
    /// <summary>
    /// MP���񕜂���
    /// </summary>
    public void HealMP(int hl)
    {
        if(hl > 0)
        {
            AudioManager.instance.SoundMPHeal();
        }
        model.HealMP(hl);
        view.ReShowMP(model);
    }
    /// <summary>
    /// ���炩�̌��ʂɂ��MaxMP����������Ƃ��̏���
    /// </summary>
    /// <param name="up"></param>
    public void ChangeMaxMP(int up)
    {
        //�^�[���J�n���̍ő�MP�����́Amodel.ResetMP()�̒��ōs���Ă���̂ŁA���ʉ��͖�Ȃ�
        if (up > 0) { AudioManager.instance.SoundMPBuff(); } 
        else if (up < 0) { AudioManager.instance.SoundMPDeBuff(); }
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
    /// <summary>
    /// �O���o�t
    /// </summary>
    public DelCCExTernal ccExternalBuff = null;
    /// <summary>
    /// �O���o�t
    /// </summary>
    public DelCCExTernal ccExternalDrawBuff = null;
}
/// <summary>
/// �O���o�t��ێ����邽�߂̃f���Q�[�g
/// </summary>
/// <param name="card"></param>
public delegate void DelCCExTernal(CardController card);
/// <summary>
/// �J�[�h�ƃq�[���[�𓯎��ɏ����ł���悤�ɂ��邽�߂̊��N���X
/// </summary>
public class Controller : MonoBehaviour { }