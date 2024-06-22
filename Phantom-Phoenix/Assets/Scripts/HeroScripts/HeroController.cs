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
    /// �_���[�W���󂯂�
    /// </summary>
    /// <param name="enemyCard"></param>
    public void Damage(int atk)
    {
        model.Damage(atk);
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
        else { AudioManager.instance.SoundCardDeBuff(); }
        model.ChangeMaxMP(up);
        view.ReShowMP(model);
    }
}
/// <summary>
/// �J�[�h�ƃq�[���[�𓯎��ɏ����ł���悤�ɂ��邽�߂̊��N���X
/// </summary>
public class Controller : MonoBehaviour { }