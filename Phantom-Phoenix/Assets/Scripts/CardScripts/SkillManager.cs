using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static CardEntity;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance { get; private set; }
    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] HeroController _playerHeroController, _enemyHeroController;
    [SerializeField] CardController cardPrefab;
    public HeroController playerHeroController {  get { return _playerHeroController; } private set { _playerHeroController = value; } }
    public HeroController enemyHeroController { get { return _enemyHeroController; } private set { _enemyHeroController = value; } }
    [SerializeField] TensionController playerTensionController, enemyTensionController;
    [SerializeField] Transform playerHand, enemyHand;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void SkillCausedByTension(int[] fieldsID)
    {
        FieldManager.instance.GetUnitsByFieldID(fieldsID)?.ForEach(i => { i.ExecuteTensionSkill(); });
    }
    public void SkillCausedBySpellUsed(bool isPlayer)
    {
        if (isPlayer) { playerHeroController.ExecuteSpellUsedSkill(); } else { enemyHeroController.ExecuteSpellUsedSkill(); }
        int[] fieldsID = (isPlayer ? Enumerable.Range(1, 6) : Enumerable.Range(7, 6)).ToArray();
        FieldManager.instance.GetUnitsByFieldID(fieldsID)?.ForEach(i => { i.ExecuteSpellUsedSkill(); });
    }
    #region 5大スキル
    public bool IsFast(CardModel model) //即撃
    {
        if (model.skill1 == CardEntity.Skill.fast || model.skill2 == CardEntity.Skill.fast || model.skill3 == CardEntity.Skill.fast || model.addSkills.Any(i => i == CardEntity.Skill.fast))
        {
            return true;
        }
        return false;
    }
    public bool hasTaunt(CardModel model)
    {
        if (model.skill1 == CardEntity.Skill.taunt || model.skill2 == CardEntity.Skill.taunt || model.skill3 == CardEntity.Skill.taunt || model.addSkills.Any(i => i == CardEntity.Skill.taunt))
        {
            return true;
        }
        return false;
    }
    public bool IsTaunt(CardModel model) //挑発
    {
        if (hasTaunt(model))
        {
            //敵なら7,8,9が前列となり、味方なら1,2,3が前列となる
            if (!model.isPlayerCard && (model.thisFieldID == 7 || model.thisFieldID == 8 || model.thisFieldID == 9)
                || (model.isPlayerCard && (model.thisFieldID == 1 || model.thisFieldID == 2 || model.thisFieldID == 3))
                )
            {
                return true;
            }
        }
        return false;
    }
    public bool IsSnipe(CardModel model) //狙撃
    {
        if (!model.isSeal && (model.skill1 == CardEntity.Skill.snipe || model.skill2 == CardEntity.Skill.snipe || model.skill3 == CardEntity.Skill.snipe) || model.addSkills.Any(i => i == CardEntity.Skill.snipe))
        {
            return true;
        }
        return false;
    }
    public bool IsPierce(CardModel model) //貫通
    {
        if (!model.isSeal && (model.skill1 == CardEntity.Skill.pierce || model.skill2 == CardEntity.Skill.pierce || model.skill3 == CardEntity.Skill.pierce || model.addSkills.Any(i => i == CardEntity.Skill.pierce)))
        {
            return true;
        }
        return false;
    }
    public bool IsActiveDoubleAction(CardModel model) //連撃権
    {
        if (!model.isSeal &&  IsDoubleAction(model))
        {
            if (model.isActiveDoubleAction) { return true; }
        }
        return false;
    }
    public bool IsDoubleAction(CardModel model) //連撃
    {
        if (!model.isSeal && ( model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction || model.addSkills.Any(i => i == CardEntity.Skill.doubleAction)))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 封印用　連撃効果を持っていると、召喚時とターン開始時に連撃権がtrueとなる  つまり、連撃効果持ちで連撃権がfalseの時は、1回目の攻撃を済ませている    よって、その時は行動できないようにしたい
    /// </summary>
    public bool HasDoubleActionAndIsNotActiveDoubleAction(CardModel model)
    {
        if (!model.isActiveDoubleAction && (model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction || model.addSkills.Any
            (i => i == CardEntity.Skill.doubleAction) ))
        {
            return true;
        }
        return false;
    }
    #endregion
    /// <summary>
    /// 何らかの挑発が存在しているか
    /// </summary>
    public bool IsAnyTaunt(bool isPlayerField)
    {

        if (isPlayerField)
        {
            //playerFieldsのfieldID1,2,3のうち、カードが紐づいているフィールドを抽出し、そのカード群の中に isTaunt == true なものがあるなら、trueを返す
            if (playerFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.isTaunt).Count() > 0) { return true; }
        }
        else
        {
            //enemyFieldsのfieldID1,2,3のうち、カードが紐づいているフィールドを抽出し、そのカード群の中に isTaunt == true なものがあるなら、trueを返す
            if (enemyFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.isTaunt).Count() > 0) { return true; }
        }
        return false;
    }
    /// <summary>
    /// ブロックされているか
    /// </summary>
    public bool IsBlock(bool isPlayerField, int thisFieldID)
    {
        //fieldIDは、　
        //             後列前列    前列後列
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //となっている
        if (isPlayerField)
        {
            //ブロック
            //GetUnitByFieldID()で取得したユニットがnullではないなら、すぐ前にユニットがいるためブロックが成立している
            if ((thisFieldID == 4 && FieldManager.instance.GetUnitByFieldID(1) != null)
                || (thisFieldID == 5 && FieldManager.instance.GetUnitByFieldID(2) != null)
                || (thisFieldID == 6 && FieldManager.instance.GetUnitByFieldID(3) != null)
                ) { return true; }
        }
        else
        {
            //ブロック
            if ((thisFieldID == 10 && FieldManager.instance.GetUnitByFieldID(7) != null)
                || (thisFieldID == 11 && FieldManager.instance.GetUnitByFieldID(8) != null)
                || (thisFieldID == 12 && FieldManager.instance.GetUnitByFieldID(9) != null)
                ) { return true; }
        }

        return false;
    }
    /// <summary>
    /// ウォールとなっているか
    /// </summary>
    public bool IsWall(bool isPlayerField)
    {
        if (isPlayerField
            && (FieldManager.instance.GetUnitByFieldID(1) != null || FieldManager.instance.GetUnitByFieldID(4) != null)
            && (FieldManager.instance.GetUnitByFieldID(2) != null || FieldManager.instance.GetUnitByFieldID(5) != null)
            && (FieldManager.instance.GetUnitByFieldID(3) != null || FieldManager.instance.GetUnitByFieldID(6) != null)
            )
        {
            return true;
        }
        else if (!isPlayerField
            && (FieldManager.instance.GetUnitByFieldID(7) != null || FieldManager.instance.GetUnitByFieldID(10) != null)
            && (FieldManager.instance.GetUnitByFieldID(8) != null || FieldManager.instance.GetUnitByFieldID(11) != null)
            && (FieldManager.instance.GetUnitByFieldID(9) != null || FieldManager.instance.GetUnitByFieldID(12) != null)
            )
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 貫通効果の発動
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public void ExecutePierce(CardController attacker, CardController target)
    {
        int targetFieldID = target.model.thisFieldID;
        if (IsPierce(attacker.model)
            && (
                (1 <= targetFieldID && targetFieldID <= 3)
                || (7 <= targetFieldID && targetFieldID <= 9)
            )
            )
        {
            FieldManager.instance.GetUnitByFieldID(target.model.thisFieldID + 3)?.Damage(attacker.model.atk);
        }　//前列のfieldIDに+3したら、後列のfieldIDになる
    }
    /// <summary>
    /// テンション貫通効果の発動
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public void ExecutePierce(int dmg,CardController target)
    {
        int targetFieldID = target.model.thisFieldID;
        if(
            (1 <= targetFieldID && targetFieldID <= 3)
            || (7 <= targetFieldID && targetFieldID <= 9)
           )
        {
            FieldManager.instance.GetUnitByFieldID(target.model.thisFieldID + 3)?.Damage(dmg);
        }　//前列のfieldIDに+3したら、後列のfieldIDになる
    }
    /// <summary>
    /// 各種カードの特殊効果を羅列する
    /// </summary>
    /// <param name="c"></param>
    /// <param name="targets"></param>
    public void SpecialSkills(CardController c, CardController[] targets = null, HeroController hctarget = null)
    {
        HeroController h = c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        HeroController eh = !c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        TensionController t = c.model.isPlayerCard ? playerTensionController : enemyTensionController;
        TensionController et = !c.model.isPlayerCard ? playerTensionController : enemyTensionController;

        switch (c.model.cardID)
        {
            #region トークン以外
            //ucommon011
            case 1: { break; } //なし
            //ucommon122
            case 2: { break; } //なし
            //ucommon102
            case 3: { break; } //挑発
            //ucommon121
            case 4: { break; } //狙撃
            //Dwarf
            case 5: { break; } //即撃狙撃連撃
            //Behemoth
            case 6: //召喚時:味方ヒーローのMP-1
                {
                    h.ChangeMaxMP(-1);
                    break;
                }
            //ucommon222
            case 7: { break; } //挑発
            //ucommon232
            case 8: { break; } //貫通
            //Cerberus
            case 9: { break; } //即撃
            //ucommon321
            case 10: //両ヒーローはカードを2枚引く
                {
                    GameManager.instance.GiveCards(h.model.isPlayer, 2);
                    GameManager.instance.GiveCards(!h.model.isPlayer, 2);
                    break;
                }
            //ucommon333 
            case 11: //味方ターン終了時:HP1回復
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            c.Heal(1);
                        }
                    };
                    break;
                }
            //ucommon322
            case 12: //味方ターン終了時:ランダムな敵ユニット1体に1ダメージ
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                            if (x != null)
                            {
                                x.Damage(1);
                            }
                        }
                    };
                    break;
                }
            //FireLord
            case 13: //召喚時&死亡時:全てのユニットに1ダメージ
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if (x.Count != 0)
                    {
                        x.ForEach(i => i.Damage(1));
                    }
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            x.ForEach(i => i.Damage(1));
                        }
                    };
                    break;
                }
            //ucommon445
            case 14: { break; }　//なし
            //ucommon443
            case 15: //召喚時:敵ユニット1体に2ダメージ
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (x != null)
                        {
                            x.Damage(2);
                        }
                    }
                    else if (targets != null)
                    {
                        targets.First().Damage(2);
                    }
                    break;
                }
            //ucommon434
            case 16: //召喚時:ユニット1体を封印する
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (x != null)
                        {
                            x.SetIsSeal(true);
                        }
                        else if (FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard) is var y)
                        {
                            if (y != null)
                            {
                                y.SetIsSeal(true);
                            }
                        }
                    }
                    else if (targets != null)
                    {
                        targets.First().SetIsSeal(true);
                    }
                    break;
                }
            //king
            case 17: { h.Heal(5); break; } //召喚時:味方ヒーローのHP5回復
            //ucommon501
            case 18: //死亡時:全ての敵ユニットを燃焼させる
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(7, 6).ToArray() : Enumerable.Range(1, 6).ToArray()
                            ).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            //対象ユニットのターン終了時特殊スキルを追加する
                            x.ForEach(i =>
                            {
                                i.SetIsBurning(true);
                                i.SpecialSkillEndTurn += (bool isPlayerTurn) => { i.Damage(1); };
                            });
                        }
                    };
                    break;
                }
            //ucommon517
            case 19: //貫通 攻撃時:味方ヒーローはカードを1枚引く
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        if (isAttacker) { GameManager.instance.GiveCards(h.model.isPlayer, 1); }
                    };
                    break;
                }
            //Driller
            case 20: //召喚時:このバトル中、元のATKが2以下の味方ユニットは+1/+1
                {
                    //Drillerは2/2なので対象
                    c.Buff(1, 1);
                    //フィールドの対象ユニットを+1/+1
                    FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(1, 6).ToArray() : Enumerable.Range(7, 6).ToArray()
                            ).Where(i => i.model.defaultATK <= 2 && i.model.thisFieldID != c.model.thisFieldID).ToList()?.ForEach(i => i.Buff(1, 1));
                    //手札の対象カードを+1/+1
                    FieldManager.instance.GetCardsInHand(c.model.isPlayerCard)?.Where(i => i.model.defaultATK <= 2).ToList().ForEach(i => i.SilentBuff(1, 1));
                    //今後引くカードを+1/+1するように
                    h.ccExternalBuff += (CardController cc) => { if (cc.model.defaultATK <= 2) { cc.SilentBuff(1, 1); } };
                    break;
                }
            //ucommon656
            case 21: //狙撃
                { break; }
            case 22: //召喚時&味方ターン終了時: 味方フィールドに、cardID1の0/1/1を1体出す
                {
                    void SummonUnit011()
                    {
                        if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard) is var x && x.emptyField != null)
                        {
                            CardController cc = Instantiate(cardPrefab, x.emptyField);
                            cc.Init(1, c.model.isPlayerCard); // cardID1 = unit011;
                            cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                        }
                    }

                    SummonUnit011();
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            SummonUnit011();
                        }
                    };
                    break;
                }
            //ucommon633
            case 23: { break; } //即撃 味方フィールドの\nユニットの数分、コスト-1
            //ucommon777
            case 24: { break; } //挑発
            //ucommon746
            case 25: //ユニットを選択　味方なら4回復 敵なら4ダメージ
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (x != null)
                        {
                            x.Damage(4);
                        }
                        else if (FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard) is var y)
                        {
                            if (y != null)
                            {
                                y.Heal(4);
                            }
                        }
                    }
                    else if (targets != null)
                    {
                        var x = targets.First();
                        if (x.model.isPlayerCard == c.model.isPlayerCard) { x.Heal(4); }
                        else { x.Damage(4); }
                    }
                    break;
                }
            //undead
            case 26: //挑発 貫通 攻撃時:死亡する 前列死亡時:後列に復活する 後列死亡時:前列に復活する 
                {
                    void SummonUndead((Transform emptyField, int fieldID) z)
                    {
                        CardController cc = Instantiate(cardPrefab, z.emptyField);
                        cc.Init(c.model.cardID, c.model.isPlayerCard);
                        cc.SummonOnField(z.fieldID, ExecuteReduceMP: false);
                    }

                    c.SpecialSkillBeforeDie = () =>
                    {
                        bool isFront = FieldManager.instance.IsFront(c.model.thisFieldID);
                        if (isFront && FieldManager.instance.GetEmptyBackFieldID(c.model.isPlayerCard) is var x && x.emptyField != null)
                        {
                            SummonUndead(x);
                        }
                        else if (!isFront && FieldManager.instance.GetEmptyFrontFieldID(c.model.isPlayerCard) is var y && y.emptyField != null)
                        {
                            SummonUndead(y);
                        }
                    };
                    break;
                }
            //ucommon877
            case 27: //ATKを1～13のランダムな値にして、HPをATK-13の値にする
                {
                    var i = UnityEngine.Random.Range(1, 14);
                    if (i > 7)
                    {
                        c.DeBuff(0, i - 7);
                        c.Buff(i - 7, 0);
                    }
                    else
                    {
                        c.DeBuff(i - 7, 0);
                        c.Buff(0, i - 7);
                    }
                    break;
                }
            //ucommon863
            case 28: //ランダムな味方ユニット1体を死亡させたら、ランダムな敵ユニット1体を死亡させる
                {
                    if (FieldManager.instance.GetRandomUnits(c.model.isPlayerCard, c) is var x && x != null)
                    {
                        x.Damage(99);
                        if (FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard) is var y && y != null)
                        {
                            y.Damage(99);
                        }
                    }

                    break;
                }
            //gargoyle
            case 29: //死亡時:全ての味方ユニットに狙撃を付与
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(1, 6).ToArray() : Enumerable.Range(7, 6).ToArray()
                            ).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            //対象ユニットに狙撃を付与する
                            x.ForEach(i =>
                            {
                                i.SetIsSnipe(true);
                            });
                        }
                    };
                    break;
                }
            //ucommon925
            case 30: //召喚時:全てのユニットに4ダメージ
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if (x.Count != 0)
                    {
                        x.ForEach(i => i.Damage(4));
                    }
                    break;
                }
            //ucommon945
            case 31: //召喚時:cardID3,4,7,8を出す
                {
                    bool SummonCard(int cardID)
                    {
                        if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard) is var x && x.emptyField != null)
                        {
                            CardController cc = Instantiate(cardPrefab, x.emptyField);
                            cc.Init(cardID, c.model.isPlayerCard); // cardID1 = unit011;
                            cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                            return true;
                        }
                        return false;
                    }

                    if (SummonCard(3)) { if (SummonCard(4)) { if (SummonCard(7)) { SummonCard(8); } } }
                    break;
                }
            //hellhound 
            case 32: //味方+2/+2 敵-2/-2
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if (x.Count != 0)
                    {
                        x.ForEach(i =>
                        {
                            if (i.model.isPlayerCard == c.model.isPlayerCard)
                            {
                                i.Buff(2, 2);
                            }
                            else
                            {
                                i.DeBuff(2, 2);
                            }
                        });
                    }
                    break;
                }
            //エルフトークンユニットの召喚処理
            void Summontelf122(int avoidFieldID = 99)
            {
                if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard, avoidFieldID) is var x && x.emptyField != null)
                {
                    CardController cc = Instantiate(cardPrefab, x.emptyField);
                    cc.Init(10002, c.model.isPlayerCard); // cardID10002 = telf122;
                    cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                }
            }
            void Summontelf122ByFieldID(Transform field, int fieldID, bool changePlayerCard = false)
            {
                CardController cc = Instantiate(cardPrefab, field);
                cc.Init(10002, changePlayerCard ? !c.model.isPlayerCard : c.model.isPlayerCard);
                cc.SummonOnField(fieldID, ExecuteReduceMP: false);
            }
            //uelf101 
            case 33: //死亡時:telf122を出す
                {

                    c.SpecialSkillBeforeDie = () =>
                    {
                        Summontelf122ByFieldID(c.transform.parent, c.model.thisFieldID);
                    };
                    break;
                }
            //uelf221
            case 34: //テンション上昇時:味方フィールドにtelf122を出す
                {

                    c.TensionSkill = () =>
                    {
                        Summontelf122();
                    };
                    break;
                }
            //uelf212
            case 35: //召喚時:1コスト以下の味方ユニットが居るなら+1/+1
                {
                    if(FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1)?.Count() != 0)
                    {
                        c.Buff(1, 1);
                    }
                    break;
                }
            //uelf222
            case 36: //攻撃時:味方フィールドにtelf122を出す
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        if (isAttacker)
                        {
                            Summontelf122(c.model.thisFieldID);
                        }
                    };
                    break;
                }
            //self2
            case 37: //テンション+1 カードを1枚引く
                {
                    c.SpellContents = () =>
                    {
                        t.SetTension(t.model.tension + 1);
                        GameManager.instance.GiveCards(c.model.isPlayerCard, 1);
                    };
                    break;
                }
            //uelf312
            case 38: //このユニットの上下にtelf122を出す
                {
                    FieldManager.instance.GetEmptyUpDownFieldID(c.model.thisFieldID)?.Where(i => i.emptyField != null).ToList().ForEach(i => Summontelf122ByFieldID(i.emptyField, i.fieldID));
                    break;
                }
            //uelf301
            case 39: //1コスト以下の味方ユニットを死亡させ、その数分+1/+2
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        x.Where(i => i.model.defaultCost <= 1).ToList().ForEach(i =>
                        {
                            i.Damage(99);
                            c.Buff(1, 2);
                        });
                    }
                    break;
                }
            //self3
            case 40: //敵ユニット1体に0ダメージ　1コスト以下の味方ユニットの数分ダメージ+1
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        var plusDamage = 0;
                        if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                        {
                            plusDamage = x.Where(i => i.model.defaultCost <= 1).Count();
                        }
                        cc.DamageFromSpell(0 + plusDamage, c.model.isPlayerCard); ;
                    };
                    break;
                }
            //guarda
            case 41: //1コスト以下の味方ユニットの数分ATK+1
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1).Count() is var cnt && cnt >= 1)
                    {
                        c.Buff(cnt, 0);
                    }
                    break;
                }
            //guardb
            case 42: //1コスト以下の味方ユニットの数分HP+1
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1).Count() is var cnt && cnt >= 1)
                    {
                        c.Buff(0, cnt);
                    }
                    break;
                }
            //uelf423
            case 43: //味方ヒーローのHPを1回復 1コスト以下の味方ユニットの数分、回復量+1
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1).Count() is var cnt && cnt >= 1)
                    {
                        h.Heal(1 + cnt);
                    }
                    else
                    {
                        h.Heal(1);
                    }
                    break;
                }
            //uelf416
            case 44: //被攻撃時:telf122を出す
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        Summontelf122(c.model.thisFieldID);
                    };
                    break;
                }
            //unit433
            case 45: //1コスト以下の味方ユニットを死亡させ、その数分カードを引く
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        var y = x.Where(i => i.model.defaultCost <= 1);
                        if(y.Count() is var i && i > 0)GameManager.instance.GiveCards(c.model.isPlayerCard, i);
                        
                    }
                    break;
                }
            //self4
            case 46: //全ての敵ユニットに1ダメージ telf122を3体出す
                {
                    c.SpellContents = () =>
                    {
                        if (FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard) is var x && x != null)
                        {
                            x.ForEach(i => i.DamageFromSpell(1, c.model.isPlayerCard));
                        }
                        Summontelf122();
                        Summontelf122();
                        Summontelf122();
                    };
                    break;
                }
            //uelf542
            case 47: //全てのフィールドにtelf122を出す
                {
                    FieldManager.instance.GetEmptyFieldIDs(c.model.isPlayerCard).ForEach(i => Summontelf122ByFieldID(i.emptyField, i.fieldID)); //味方フィールド
                    FieldManager.instance.GetEmptyFieldIDs(!c.model.isPlayerCard).ForEach(i => Summontelf122ByFieldID(i.emptyField, i.fieldID, true)); //敵フィールド
                    break;
                }
            //uelf525
            case 48: //味方ターン終了時:前列にいるならtelf122を2体出す
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            if(c.model.thisFieldID % 6 == 1 || c.model.thisFieldID % 6 == 2 || c.model.thisFieldID % 6 == 3)
                            {
                                Summontelf122();
                                Summontelf122();
                            }
                        }
                    };
                    break;
                }
            //uelf622
            case 49: //敵1体に1ダメージ 1コスト以下の味方ユニットの数分ダメージ+1
                {
                    var plusDamage = 0;
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        plusDamage = x.Where(i => i.model.defaultCost <= 1).Count();
                    }

                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var y = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (y != null)
                        {
                            y.Damage(1 + plusDamage);
                        }
                        else
                        {
                            playerHeroController.Damage(1 + plusDamage);
                        }
                    }
                    else if (targets != null)
                    {
                        targets.First().Damage(1 + plusDamage);
                    }
                    else if(hctarget != null)
                    {
                        hctarget.Damage(1 + plusDamage);
                    }
                    break;

                }
            //uelf633
            case 50: //テンション上昇時:味方フィールドにtelf122を2体出す
                {

                    c.TensionSkill = () =>
                    {
                        Summontelf122(); Summontelf122();
                    };
                    break;
                }
            //uelf711
            case 51: { break; } //この対戦中に死亡した1コスト以下の味方ユニットの数分+1/+1
            //uelf955
            case 52: { break; } //味方フィールドの\n1コスト以下の味方ユニットの数分、コスト-1
            //switch0
            case 53: //1ダメージ
            {
                c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(1, c.model.isPlayerCard); };
                c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(1, c.model.isPlayerCard); };
                break;
            }
            //switch1
            case 54: //全ての敵ユニットに1ダメージ
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DamageFromSpell(1, c.model.isPlayerCard));
                    };
                    break;
                }
            //uwitch221
            case 55: //死亡時:デッキからスペルを1枚引く
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        GameManager.instance.GiveSearchCards(c.model.isPlayerCard, 1, (i) => { return GameDataManager.instance.cardlist.cl[i - 1].category == Category.spell; });
                        
                    };
                    break;
                }
            //uwitch223
            case 56: //スペル使用時:ATK+1
                {
                    c.SpellUsedSkill = () =>
                    {
                        c.Buff(1, 0);
                    };
                    break;
                }
            //switch2d
            case 57: //2ダメージ
                {
                    c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(2, c.model.isPlayerCard); };
                    c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(2, c.model.isPlayerCard); };
                    break;
                }
            //switch2h
            case 58: //3回復　カードを1枚引く
                {
                    c.hcSpellContents = (HeroController hc) => { hc.Heal(3); GameManager.instance.GiveCards(c.model.isPlayerCard, 1); };
                    c.ccSpellContents = (CardController cc) => { cc.Heal(3); GameManager.instance.GiveCards(c.model.isPlayerCard, 1); };
                    break;
                }
            //uwitch315
            case 59: //スペルダメージ+1
                {
                    h.spellDamageBuff(1);
                    c.SpecialSkillBeforeDie = () =>
                    {
                        h.spellDamageBuff(-1);
                    };
                    break;
                }
            //iceMaiden
            case 60: //挑発　召喚時&死亡時:全ての味方を1回復する
                {
                    if (c.model.isPlayerCard) { playerHeroController.Heal(1); }
                    else { enemyHeroController.Heal(1); }

                    var x = FieldManager.instance.GetUnitsByFieldID((c.model.isPlayerCard ? Enumerable.Range(1, 6) : Enumerable.Range(7, 6)).ToArray())
                        .Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if (x.Count != 0)
                    {
                        x.ForEach(i => i.Heal(1));
                    }
                    c.SpecialSkillBeforeDie = () =>
                    {
                        if (c.model.isPlayerCard) { playerHeroController.Heal(1); }
                        else { enemyHeroController.Heal(1); }

                        var x = FieldManager.instance.GetUnitsByFieldID((c.model.isPlayerCard ? Enumerable.Range(1, 6) : Enumerable.Range(7, 6)).ToArray())
                        .Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            x.ForEach(i => i.Heal(1));
                        }
                    };
                    break;
                }
            //uwitch425
            case 61: //スペル使用時:HP+1
                {
                    c.SpellUsedSkill = () =>
                    {
                        c.Buff(0, 1);
                    };
                    break;
                }
            //uwitch433
            case 62: //スペル使用時:前列にいるなら、テンション+1
                {
                    c.SpellUsedSkill = () =>
                    {
                        if (c.model.thisFieldID % 6 == 1 || c.model.thisFieldID % 6 == 2 || c.model.thisFieldID % 6 == 3)
                        {
                            t.SetTension(t.model.tension + 1);
                        }
                    };
                    break;
                }
            //switch4d
            case 63: //4ダメージ
                {
                    c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(4, c.model.isPlayerCard); };
                    c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(4, c.model.isPlayerCard); };
                    break;
                }
            //switch4h
            case 64: //6回復　カードを2枚引く
                {
                    c.hcSpellContents = (HeroController hc) => { hc.Heal(6); GameManager.instance.GiveCards(c.model.isPlayerCard, 2); };
                    c.ccSpellContents = (CardController cc) => { cc.Heal(6); GameManager.instance.GiveCards(c.model.isPlayerCard, 2); };
                    break;
                }
            //uwitch545
            case 65: //味方ターン終了時 味方ヒーローのHPを2回復　ランダムな敵ユニットに1ダメージ
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            h.Heal(2);

                            var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                            if (x != null)
                            {
                                x.Damage(1);
                            }
                        }
                    };
                    break;
                }
            //switch5
            case 66: //コスト3以下の全ての敵ユニットを死亡させる
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).Where(x => x.model.defaultCost <= 3).ToList().ForEach(i => i.Damage(99));
                    };
                    break;
                }
            //uwitch624
            case 67: //即撃 狙撃 この対戦中に使用したスペルの数分、コスト-1
                {
                    break;
                }
            //zodiac
            case 68: //両ヒーローに7ダメージ
                {
                    h.Damage(7);
                    eh.Damage(7);
                    break;
                }
            //uwitch744
            case 69: //召喚時:手札の全てのスペルのコスト-1
                {
                    FieldManager.instance.GetCardsInHand(c.model.isPlayerCard).Where(i => i.model.category == Category.spell).ToList().ForEach(i => i.CreaseCost(-2));
                    break;
                }
            //switch7
            case 70: //全ての敵ユニットに5ダメージ
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DamageFromSpell(5, c.model.isPlayerCard));
                    };
                    break;
                }
            //switch10
            case 71: //全ての敵に2ダメージ　テンション3なら、ダメージ+3&テンション-3
                {
                    c.SpellContents = () =>
                    {
                        var plusDamage = 0;
                        if(t.model.tension == 3)
                        {
                            plusDamage = 3;
                            t.SetTension(0);
                        }
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DamageFromSpell(2 + plusDamage, c.model.isPlayerCard));
                        eh.Damage(2 + plusDamage);
                    };
                    break;
                }
            //switch20
            case 72: //6ダメージ この対戦中に使用したスペルの数分コスト-1
                {
                    c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(6, c.model.isPlayerCard); };
                    c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(6, c.model.isPlayerCard); };
                    break;
                }
            //キングアイテムカードの手札追加処理
            void GetItemCard(int cnt)
            {
                    if(cnt <= 0) {  return; }
                    int[] cardIDs = Enumerable.Range(0, cnt).Select(i => UnityEngine.Random.Range(10003, 10006)).ToArray();
                    GameManager.instance.GiveSpecificCards(c.model.isPlayerCard, cardIDs);
            }
            //uking111
            case 73: //召喚時:味方ユニットを+1/+1
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        FieldManager.instance.GetRandomUnits(false, c)?.Buff(1, 1);
                    }
                    targets?.First().Buff(1, 1);
                    break;
                }
            //sking1
            case 74: //アイテムカードを2～3枚加える
                {
                    c.SpellContents = () =>
                    {
                        GetItemCard(UnityEngine.Random.Range(2,4));
                    };
                    break;
                }
            //uking222
            case 75: //召喚時:アイテムカードを1～2枚加える
                {
                    GetItemCard(UnityEngine.Random.Range(1, 3));
                    break;
                }
            //uking211
            case 76: //召喚時:味方ユニットがいないなら2/1/1を2体出す
                {
                    void Summonk211()
                    {
                        if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard) is var x && x.emptyField != null)
                        {
                            CardController cc = Instantiate(cardPrefab, x.emptyField);
                            cc.Init(c.model.cardID, c.model.isPlayerCard);
                            cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                        }
                    }

                    if (FieldManager.instance.GetRandomUnits(c.model.isPlayerCard, c) == null)
                    {
                        Summonk211();
                        Summonk211();
                    }
                    break;
                }
            //sking2taunt
            case 77: //挑発付与 HP+2
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        cc.SetIsTaunt(true);
                        cc.Buff(0, 2);
                    };
                    break;
                }
            //sking2pierce
            case 78: //貫通付与 ATK+2
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        cc.SetIsPierce(true);
                        cc.Buff(2, 0);
                    };
                    break;
                }
            //uking312
            case 79: //被強化時:ATK+1
                {
                    c.SpecialSkillAfterBuff = () =>
                    {
                        c.Buff(1, 0, false);
                    };
                    break;
                }
            //uking322
            case 80: //召喚時:味方ユニット1体のHP+1 挑発付与
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var x = FieldManager.instance.GetRandomUnits(c.model.isPlayerCard, c);
                        if (x != null)
                        {
                            x.Buff(0,1);
                            x.SetIsTaunt(true);
                        }
                    }
                    else if (targets != null)
                    {
                        var x = targets.First();
                        x.Buff(0, 1);
                        x.SetIsTaunt(true);
                    }
                    break;
                }
            //itemer
            case 81: //召喚時&死亡時&味方ターン終了時: アイテムカードを1枚加える
                {
                    GetItemCard(1);
                    c.SpecialSkillBeforeDie = () =>
                    {
                        GetItemCard(1);
                    };
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if(isPlayerTurn == c.model.isPlayerCard)
                        {
                            GetItemCard(1);
                        }
                    };
                    break;
                }
            //uking422
            case 82: //テンション上昇時:自身以外のランダムな味方ユニットを+1/+1
                {
                    c.TensionSkill = () =>
                    {
                        var x = FieldManager.instance.GetRandomUnits(c.model.isPlayerCard, c);
                        if (x != null)
                        {
                            x.Buff(1,1);
                        }
                    };
                    break;
                }
            //uking431
            case 83: //被強化時:ランダムな敵ユニットに2ダメージ
                {
                    c.SpecialSkillAfterBuff = () =>
                    {
                        var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (x != null)
                        {
                            x.Damage(2);
                        }
                    };
                    break;
                }
            //uking445
            case 84: //行動できない 味方ターン終了時:アイテムカードを1枚加える
                {
                    c.SetHasCannotAttack(true);
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            GetItemCard(1);
                        }
                    };
                    break;
                }
            //uking523
            case 85: //召喚時:全ての味方ユニットを+1/+1
                {
                    if(FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList() is var x && x != null){
                        x.ForEach(i => i.Buff(1,1));
                    }
                    break;
                }
            //uking612
            case 86: //召喚時:ランダムな敵ユニットに1ダメージ この対戦中にアイテムカードで強化を行った回数分繰り返す
                {
                    for (int i = 0; i < (c.model.isPlayerCard ? FieldManager.instance.playerBuffedCntByItemCard : FieldManager.instance.enemyBuffedCntByItemCard); i++)
                    {
                        FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard).Damage(1);
                    }
                    break;
                }
            //uking644
            case 87: //召喚時:味方テンションの数分、味方ユニット1体をバフする
                {
                    var x = t.model.tension;
                    targets?.First().Buff(x,x);
                    break;
                }
            //uking723
            case 88: //被強化時:7/2/3を出す
                {
                    void Summonk723()
                    {
                        if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard) is var x && x.emptyField != null)
                        {
                            CardController cc = Instantiate(cardPrefab, x.emptyField);
                            cc.Init(c.model.cardID, c.model.isPlayerCard);
                            cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                        }
                    }

                    c.SpecialSkillAfterBuff = () =>
                    {
                        Summonk723();
                    };
                    break;
                }
            //uking722
            case 89: //後列の味方ユニットを全て死亡させ、HPとATKを吸収する
                {
                    int plusATK = 0;
                    int plusHP = 0;
                    var x = FieldManager.instance.GetUnitsByFieldID(c.model.isPlayerCard ? Enumerable.Range(4, 3).ToArray() : Enumerable.Range(10, 3).ToArray())
                        .Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if(x != null){
                        x.ForEach(i => {
                            plusATK += i.model.atk;
                            plusHP += i.model.hp;
                            i.Damage(99);
                        }); 
                    }
                    if(plusATK == 0 && plusHP == 00) { return; }
                    c.Buff(plusATK, plusHP);
                    break;
                }
            //tamer
            case 90: //味方ユニット1体のスタッツを2倍にする
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var x = FieldManager.instance.GetRandomUnits(c.model.isPlayerCard, c);
                        if (x != null)
                        {
                            x.Buff(x.model.atk, x.model.hp);
                        }
                    }
                    else if (targets != null)
                    {
                        var x = targets.First();
                        x.Buff(x.model.atk, x.model.hp);
                    }
                    break;
                }
            //uking834
            case 91: { break; } //アイテムカードで強化を行った回数分コスト-1
            //uking946
            case 92: //召喚時:味方ユニットが居るなら全てのユニットを+2/+2 居ないなら+5/+3
                {
                    if (FieldManager.instance.GetRandomUnits(c.model.isPlayerCard, c) != null)
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard)?.Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList().ForEach(i => i.Buff(2, 2));
                    }
                    else
                    {
                        c.Buff(5, 3);
                    }
                    break;
                }
            //sdemon0
            case 93: //味方ヒーローのMP+1
                {
                    c.SpellContents = () =>
                    {
                        h.IncreaseMP(1);
                    };
                    break;
                }
            //udemon111
            case 94: //召喚時:ユニット1体を-1/-1
                {
                    if(!GameDataManager.instance.isOnlineBattle && !GameManager.instance.isPlayerTurn)
                    {
                        FieldManager.instance.GetRandomUnits(true, c)?.DeBuff(1, 1);
                    }
                    else if(targets != null)
                    {
                        targets.First().DeBuff(1, 0);
                    }
                    break;
                }
            //sdemon1
            case 95: //味方ユニットを死亡させて、最大MP+1して、カードを1枚引く
                {
                    c.ccSpellContents = (CardController target) =>
                    {
                        target.Damage(99);
                        h.ChangeMaxMP(1);
                        GameManager.instance.GiveCards(c.model.isPlayerCard, 1);
                    };
                    break;
                }
            //udemon222
            case 96: //召喚時:敵ヒーローのテンション-1
                {
                    et.SetTension(et.model.tension - 1);
                    break;
                }
            //udemon223
            case 97: //敵ユニットカードのコスト+1
                {
                    void enemyUnitCardsCostPlus1(CardController cc)
                    {
                        if (cc.model.category == Category.unit) { cc.TemporaryCreaseCost(1); }
                    }
                    eh.ccExternalBuff += enemyUnitCardsCostPlus1; //今後手札に加わるカードに反映するように

                    //既に手札にあるカードに反映
                    FieldManager.instance.GetCardsInHand(!c.model.isPlayerCard).ForEach(i =>
                    {
                        if (i.model.category == Category.unit) { i.TemporaryCreaseCost(1); }
                    });

                    c.SpecialSkillBeforeDie += () =>
                    {
                        eh.ccExternalBuff -= enemyUnitCardsCostPlus1; //今後手札に加わるカードに反映されないように
                        FieldManager.instance.GetCardsInHand(!c.model.isPlayerCard).ForEach(i =>
                        {
                            if (i.model.category == Category.unit) { i.TemporaryCreaseCost(-1); } //既に手札にあるカードに反映されないように
                        });
                    };
                    break;
                }
            //sdemon2
            case 98: //味方ユニットを死亡させて、そのユニットのコスト分、HPとMPを回復する
                {
                    c.ccSpellContents = (CardController target) =>
                    {
                        var xCost = target.model.cost;
                        target.Damage(99);
                        h.HealMP(xCost);
                        h.Heal(xCost);
                    };
                    break;
                }
            //udemon321
            case 99: //召喚時:前列のユニットを-1/-1
                {
                    if(FieldManager.instance.GetUnitsByFieldID(new int[] { 1,2,3, 7,8,9 }) is var x && x != null)
                    {
                        x.Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList().ForEach(i => i.DeBuff(1, 1));
                    }
                    break;
                }
            //udemon312
            case 100: //召喚時:味方ユニット1体を-1/-1 そうした場合、敵ヒーローのMP-1
                {
                    if (!GameDataManager.instance.isOnlineBattle && !GameManager.instance.isPlayerTurn)
                    {
                        FieldManager.instance.GetRandomUnits(false, c)?.DeBuff(1, 1);
                        eh.ChangeMaxMP(-1);
                    }
                    else if (targets != null)
                    {
                        targets.First().DeBuff(1, 1);
                        eh.ChangeMaxMP(-1);
                    }
                    break;
                }
            //sdemon3
            case 101: //ユニット1体のATK-5
                {
                    c.ccSpellContents = (CardController target) =>
                    {
                        target.DeBuff(4, 0);
                    };
                    break;
                }
            //guardc
            case 102: //召喚時:全ての敵ユニットを-1/-1
                {
                    FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard)?.ForEach(i => i.DeBuff(1, 1));
                    break;
                }
            //udemon413
            case 103: //召喚時:敵ユニットの数分+1/+1 全ての敵を-1/-1
                {
                    if(FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard) is var x && x != null)
                    {
                        var y = x.Count;
                        if(y > 0)
                        {
                            c.Buff(y, y);
                            x.ForEach(i => i.DeBuff(1, 0));
                        }
                    }    
                    break;
                }
            //udemon432
            case 104: //召喚時:味方ヒーローのテンションの数分、カードを引く
                {
                    GameManager.instance.GiveCards(c.model.isPlayerCard, t.model.tension); //0でも大丈夫なはず...
                    break;
                }
            //udemon533
            case 105: //敵ドロー時、敵ヒーローに1ダメージ 引いたカードが9コスト以下ならコスト+1
                {
                    void enemyCardCostPlus1(CardController cc)
                    {
                        eh.Damage(1);
                        if (cc.model.cost <= 9) { cc.CreaseCost(1); }
                    }
                    eh.ccExternalDrawBuff += enemyCardCostPlus1; //今後手札に加わるカードに反映するように
                    c.SpecialSkillBeforeDie += () =>
                    {
                        eh.ccExternalDrawBuff -= enemyCardCostPlus1; //今後手札に加わるカードに反映されないように
                    };
                    break;
                }
            //udemon524
            case 106: //敵手札の8コスト以下のカードのコスト+1 これを2回繰り返す
                {
                    var x = FieldManager.instance.GetCardsInHand(!c.model.isPlayerCard).Where(i => i.model.cost <= 8)?.ToList();
                    if(x != null)
                    {
                        x[UnityEngine.Random.Range(0, x.Count())].CreaseCost(1);
                        x[UnityEngine.Random.Range(0, x.Count())].CreaseCost(1);
                    }
                    break;
                }
            //sdemon5
            case 107: //全てのユニットのATK-2 全ての敵ユニットのHP-1
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard).ForEach(i => i.DeBuff(2, 0));
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DeBuff(2, 1));
                    };
                    break;
                }
            //minotaur
            case 108: //被弱化時:同じ効果で敵ユニット全体を弱化させる この効果で被弱化時効果は発動しない
                {
                    c.SpecialSkillAfterDeBuff += (int atk, int hp) =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DeBuff(atk, hp, false)); //被弱化時効果を誘発しない　minotaurVSminotaurすると死屍累々になるので…
                    };
                    break;
                }
            //udemon634
            case 109: //召喚時:ユニット1体を-2/-2 死亡時:ランダムな敵ユニット1体を-1/-1
                {
                    if (!GameDataManager.instance.isOnlineBattle && !GameManager.instance.isPlayerTurn)
                    {
                        FieldManager.instance.GetRandomUnits(true, c)?.DeBuff(2, 2);
                    }
                    else
                    {
                        targets?.First().DeBuff(2, 2);
                    }
                    c.SpecialSkillBeforeDie = () =>
                    {
                        FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard)?.DeBuff(1, 1);
                    };
                    break;
                }
            //udemon756
            case 110: //攻撃時:敵ヒーローのテンション-2　カードを1枚引く 
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        if (isAttacker)
                        {
                            et.SetTension(et.model.tension - 2);
                            GameManager.instance.GiveCards(c.model.isPlayerCard, 1);
                        }
                    };
                    break;
                }
            //udemon728
            case 111: //ターン終了時:ランダムな敵ユニットのATKまたはHP-2
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            if(UnityEngine.Random.Range(0, 2) == 0)
                            {
                                FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard)?.DeBuff(2, 0);
                            }
                            else
                            {
                                FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard)?.DeBuff(0, 2);
                            }
                        }
                    };
                    break;
                }
            //udemon922
            case 112: //召喚時:全てのユニットを死亡させ、最大MP-3
                {
                    var hl = 0;
                    if(FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        hl += x.Count;
                        x.Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList().ForEach(i => i.Damage(99));
                    }
                    if(FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard) is var y && y != null)
                    {
                        hl += y.Count;
                        y.Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList().ForEach(i => i.Damage(99));
                    }
                    h.ChangeMaxMP(-3);
                    break;
                }
            #endregion
            #region トークン
            //telf122tension
            case 10001: { break; } //即撃
            //telf122
            case 10002: { break; }
            //tkingatk
            case 10003:
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        FieldManager.instance.AddBuffCntByItemCard(c.model.isPlayerCard);
                        cc.Buff(1,0);
                    };
                    break;
                }
            //tkinghp
            case 10004:
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        FieldManager.instance.AddBuffCntByItemCard(c.model.isPlayerCard);
                        cc.Buff(0, 1);
                    };
                    break;
                }
            //tkingatkhp
            case 10005:
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        FieldManager.instance.AddBuffCntByItemCard(c.model.isPlayerCard);
                        cc.Buff(1, 1);
                    };
                    break;
                }
                #endregion
        }

    }
    /// <summary>
    /// 手札にいる状態でも参照される、外部要因によって発生する受動的なスキルの紐づけ
    /// </summary>
    /// <param name="c"></param>
    public void UpdateSkills(CardController c)
    {
        switch (c.model.cardID)
        {
            //ucommon633 //即撃 味方フィールドの\nユニットの数分、コスト-1
            case 23:
                {
                    var recordFieldCnt = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = c.model.isPlayerCard ? FieldManager.instance.playerFieldOnUnitCnt : FieldManager.instance.enemyFieldOnUnitCnt;
                        if (recordFieldCnt != x)
                        {
                            c.CreaseCost(recordFieldCnt - x, false);
                            recordFieldCnt = x;
                        }
                    }

                    StartCoroutine(CreaseCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(CreaseCost());
                    };
                    c.SpecialSkillBeforeDie += () =>
                    {
                        StopCoroutine(CreaseCost());
                    };
                    break;
                }
            //uelf711
            case 51: //この対戦中に死亡した1コスト以下の味方ユニットの数分+1/+1
                {
                    var recordDiedCnt1 = 0;
                    StartCoroutine(ChangeStats());

                    IEnumerator ChangeStats()
                    {
                        yield return null;
                        var cnt =  (c.model.isPlayerCard ? FieldManager.instance.playerCatacombe : FieldManager.instance.enemyCatacombe).Where(x => x.cost <= 1).Count();
                        if(recordDiedCnt1 != cnt)
                        {
                            c.ChangeStats(cnt - recordDiedCnt1 + c.model.atk, cnt - recordDiedCnt1 + c.model.hp);
                            recordDiedCnt1 = cnt;
                        }
                        
                    }

                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(ChangeStats());
                    };
                    break;
                }
            //uelf955 
            case 52: //味方フィールドの\n1コスト以下の味方ユニットの数分、コスト-1
                {
                    var recordDiedCnt1 = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard).Where(i => i.model.cost <= 1).Count();
                        if (recordDiedCnt1 != x)
                        {
                            c.CreaseCost(recordDiedCnt1 - x, false);
                            recordDiedCnt1 = x;
                        }
                    }

                    StartCoroutine(CreaseCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(CreaseCost());
                    };
                    break;
                }
            //uwitch624
            case 67: //即撃 狙撃 この対戦中に使用したスペルの数分、コスト-1
                {
                    var recordSpellCount = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = (c.model.isPlayerCard ? FieldManager.instance.playerUsedSpellList : FieldManager.instance.enemyUsedSpellList).Count();
                        if (recordSpellCount != x)
                        {
                            c.CreaseCost(recordSpellCount - x, false);
                            recordSpellCount = x;
                        }
                    }

                    StartCoroutine(CreaseCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(CreaseCost());
                    };
                    break;
                }
            //switch20
            case 72: //6ダメージ 味方ヒーローに固定3ダメージ この対戦中に使用したスペルの数分コスト-1
                {
                    var recordSpellCount = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = (c.model.isPlayerCard ? FieldManager.instance.playerUsedSpellList : FieldManager.instance.enemyUsedSpellList).Count();
                        if (recordSpellCount != x)
                        {
                            c.CreaseCost(recordSpellCount - x, false);
                            recordSpellCount = x;
                        }
                    }

                    StartCoroutine(CreaseCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(CreaseCost());
                    };
                    break;
                }
            //uking834
            case 91: //アイテムカードで強化を行った回数分コスト-1
                {
                    var recordBuffByItemCard = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = (c.model.isPlayerCard ? FieldManager.instance.playerBuffedCntByItemCard : FieldManager.instance.enemyBuffedCntByItemCard);
                        if (recordBuffByItemCard != x)
                        {
                            c.CreaseCost(recordBuffByItemCard - x, false);
                            recordBuffByItemCard = x;
                        }
                    }

                    StartCoroutine(CreaseCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(CreaseCost());
                    };
                    break;
                }
        }
    }
}
