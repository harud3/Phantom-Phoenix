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
        view.Show(model);
    }
    /// <summary>
    /// �����ڂ̍ĕ\��
    /// </summary>
    private void ReShow()
    {
        view.ReShow(model);
    }
    /// <summary>
    /// �_���[�W���󂯂�
    /// </summary>
    /// <param name="enemyCard"></param>
    public void Damage(int atk)
    {
        model.Damage(atk);
        ReShow();
    }
    public void ResetMP()
    {
        model.ResetMP();
        ReShow();
    }
    public void ReduceMP(int reduce)
    {
        model.ReduceMP(reduce);
        view.ReShow(model);
    }
    public void ChangeMaxMP(int up)
    {
        model.ChangeMaxMP(up);
        view.ReShow(model);
    }
}
public class Controller : MonoBehaviour { }