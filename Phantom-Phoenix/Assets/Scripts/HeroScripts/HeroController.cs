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
    /// �����ڂ̍ĕ\��
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
    public void ResetMP()
    {
        model.ResetMP();
        ReShowHP();
    }
    public void ReduceMP(int reduce)
    {
        model.ReduceMP(reduce);
        view.ReShowMP(model);
    }
    public void ChangeMaxMP(int up)
    {
        model.ChangeMaxMP(up);
        view.ReShowMP(model);
    }
}
public class Controller : MonoBehaviour { }