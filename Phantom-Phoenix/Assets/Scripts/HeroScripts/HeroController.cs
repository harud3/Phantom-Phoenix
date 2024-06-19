using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : Controller
{
    HeroView view;
    public HeroModel model {  get; private set; }
    [SerializeField]
    private bool isPlayer; //playerかenemyか　事前にインスペクター上で指定しておく
    private void Start()
    {
        view = GetComponent<HeroView>();
    }
    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="heroID"></param>
    public void Init(int heroID)
    {
        model = new HeroModel(heroID, isPlayer);
        view.SetHero(model);
    }
    /// <summary>
    /// 見た目の再表示
    /// </summary>
    private void ReShowHP()
    {
        view.ReShowHP(model);
    }
    /// <summary>
    /// デッキ数の再表示
    /// </summary>
    public void ReShowStackCards(int deckNum)
    {
        view.ReShowStackCards(deckNum);
    }
    /// <summary>
    /// ダメージを受ける
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
        view.ReShowMP(model);
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