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
    /// ヒーローが攻撃によりダメージを受けた時の処理
    /// </summary>
    public void DamageFromAttack(int dmg)
    {
        model.Damage(dmg);
        ReShowHP();
    }
    /// <summary>
    /// ヒーローがスペルによりダメージを受けた時の処理 
    /// </summary>
    public void DamageFromSpell(int dmg)
    {
        AudioManager.instance.SoundCardFire();
        model.Damage(dmg + model.plusSpellDamage);
        ReShowHP();
        GameManager.instance.CheckIsAlive(model.isPlayer);
    }
    /// <summary>
    /// ヒーローがダメージを受けた時の処理 
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
        else { AudioManager.instance.SoundMPDeBuff(); }
        model.ChangeMaxMP(up);
        view.ReShowMP(model);
    }
    public void spellDamageBuff(int buff)
    {
        model.spellDamageBuff(buff);
    }
    public Action SpellUsedSkill = null;　//スペル使用によって発生する受動的なスキル
    public void ExecuteSpellUsedSkill()
    {
        SpellUsedSkill?.Invoke();
    }
    public Action<CardController> ccExternalBuff = new Action<CardController>((unit) => { });
}
/// <summary>
/// カードとヒーローを同時に処理できるようにするための基底クラス
/// </summary>
public class Controller : MonoBehaviour { }