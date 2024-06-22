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
    /// HPの再表示
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
    /// <summary>
    /// あきらめた時
    /// </summary>
    public void Concede()
    {
        model.Concede();
    }
    /// <summary>
    /// ターン開始時のMPリセット
    /// </summary>
    public void ResetMP()
    {
        model.ResetMP();
        view.ReShowMP(model);
    }
    /// <summary>
    /// (主にカードを出した時に)MPを減らす
    /// </summary>
    /// <param name="reduce"></param>
    public void ReduceMP(int reduce)
    {
        model.ReduceMP(reduce);
        view.ReShowMP(model);
    }
    /// <summary>
    /// 何らかの効果によりMaxMPが増減するときの処理
    /// </summary>
    /// <param name="up"></param>
    public void ChangeMaxMP(int up)
    {
        //ターン開始時の最大MP増加は、model.ResetMP()の中で行っているので、効果音は鳴らない
        if (up > 0) { } 
        else { AudioManager.instance.SoundCardDeBuff(); }
        model.ChangeMaxMP(up);
        view.ReShowMP(model);
    }
}
/// <summary>
/// カードとヒーローを同時に処理できるようにするための基底クラス
/// </summary>
public class Controller : MonoBehaviour { }