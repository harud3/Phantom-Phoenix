using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance { get; private set; }
    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] HeroController _playerHeroController, _enemyHeroController;
    public HeroController playerHeroController {  get { return _playerHeroController; } private set { _playerHeroController = value; } }
    public HeroController enemyHeroController { get { return _enemyHeroController; } private set { _enemyHeroController = value; } }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #region 5大スキル
    public bool IsFast(CardModel model) //即撃
    {
        if (model.skill1 == CardEntity.Skill.fast || model.skill2 == CardEntity.Skill.fast || model.skill3 == CardEntity.Skill.fast)
        {
            return true;
        }
        return false;
    }
    public bool IsTaunt(CardModel model) //挑発
    {
        if (model.skill1 == CardEntity.Skill.taunt || model.skill2 == CardEntity.Skill.taunt || model.skill3 == CardEntity.Skill.taunt)
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
        if (!model.isSeal && (model.skill1 == CardEntity.Skill.snipe || model.skill2 == CardEntity.Skill.snipe || model.skill3 == CardEntity.Skill.snipe))
        {
            return true;
        }
        return false;
    }
    public bool IsPierce(CardModel model) //貫通
    {
        if (!model.isSeal &&  (model.skill1 == CardEntity.Skill.pierce || model.skill2 == CardEntity.Skill.pierce || model.skill3 == CardEntity.Skill.pierce))
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
        if (!model.isSeal && ( model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction ))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 封印用　連撃効果を持っていると、召喚時とターン開始時に連撃権がtrueとなる  つまり、連撃効果持ちで連撃権がfalseの時は、1回目の攻撃を済ませている    よって、その時は行動できないようにしたい
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public bool HasDoubleActionAndIsNotActiveDoubleAction(CardModel model)
    {
        if (!model.isActiveDoubleAction && (model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction))
        {
            return true;
        }
        return false;
    }
    #endregion
    /// <summary>
    /// 何らかの挑発が存在しているか
    /// </summary>
    /// <param name="isPlayerField"></param>
    /// <returns></returns>
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
    /// <param name="isPlayerField"></param>
    /// <param name="thisFieldID"></param>
    /// <returns></returns>
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
    /// <param name="isPlayerField"></param>
    /// <returns></returns>
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
    /// 各種カードの特殊効果を羅列する
    /// </summary>
    /// <param name="c"></param>
    /// <param name="targets"></param>
    public void SpecialSkills(CardController c, CardController[] targets = null)
    {
        HeroController h = c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        switch (c.model.cardID)
        {
            ///fire スペルの書き方
            //case 10:
            //    {
            //        c.hcSpellContents = (HeroController hc) => { hc.Damage(3); };
            //        c.ccSpellContents = (CardController cc) => { cc.Damage(3); };
            //        break;
            //    }

            //122
            case 1: { break; } //なし
            //103
            case 2: { break; } //挑発
            //121
            case 3: { break; } //狙撃
            //Dwarf
            case 4: { break; } //即撃狙撃連撃
            //Behemoth
            case 5: //召喚時:味方ヒーローのMP-1
                {
                    h.ChangeMaxMP(-1);
                    break;
                }
            //223
            case 6: { break; } //挑発
            //232
            case 7: { break; } //貫通
            //Cerberus
            case 8: { break; } //即撃
            //321
            case 9: //両ヒーローはカードを2枚引く
                {
                    GameManager.instance.GivesCard(h.model.isPlayer, 2);
                    GameManager.instance.GivesCard(!h.model.isPlayer, 2);
                    break;
                }
            //333 
            case 10: //味方ターン終了時:HP1回復
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
            //322
            case 11: //味方ターン終了時:ランダムなユニット1体に1ダメージ
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
            case 12: //召喚時&死亡時:全てのユニットに2ダメージ
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if (x.Count != 0) {
                        x.ForEach(i => i.Damage(2));
                    }
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            x.ForEach(i => i.Damage(2));
                        }
                    };
                    break;
                }
            //445
            case 13: { break; }　//なし
            //443
            case 14: //召喚時:敵ユニット1体に2ダメージ
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
            //434
            case 15: //召喚時:ユニット1体を封印する
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI処理
                    {
                        var x = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (x != null)
                        {
                            x.SetIsSeal(true);
                        }
                        else if(FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard) is var y){
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
            case 16: { h.Heal(5); break; } //召喚時:味方ヒーローのHP5回復
            //501
            case 17: //死亡時:全ての敵ユニットを燃焼させる
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(7, 6).ToArray() : Enumerable.Range(1, 6).ToArray()
                            ).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            //対象ユニットのターン終了時特殊スキルを追加する
                            x.ForEach(i => i.SpecialSkillEndTurn += (bool isPlayerTurn) => { i.Damage(1); });
                        }
                    };
                    break;
                }
                //517
            case 18: //攻撃時:味方ヒーローはカードを1枚引く
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        if (isAttacker) { GameManager.instance.GivesCard(h.model.isPlayer, 1); }
                    };
                    break;
                }
                //Driller
            case 19: //召喚時:このバトル中、元のATKが2以下の味方ユニットは+2/+2
                {
                    //Drillerは2/2なので対象
                    c.Buff(2, 2);
                    //フィールドの対象ユニットを+2/+2
                    FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(1, 6).ToArray() : Enumerable.Range(7, 6).ToArray()
                            ).Where(i => i.model.defaultATK <= 2 && i.model.thisFieldID != c.model.thisFieldID).ToList()?.ForEach(i => i.Buff(2, 2));
                    //手札の対象カードを+2/+2
                    FieldManager.instance.GetUnitsInHand(c.model.isPlayerCard)?.Where(i => i.model.defaultATK <= 2).ToList().ForEach(i => i.SilentBuff(2,2) );
                    //今後引くカードを+2/+2するように
                    h.ccExternalBuff += (CardController cc) => { if (cc.model.defaultATK <= 2) { cc.SilentBuff(2, 2); } };
                    break;
                }

        }

    }
}
