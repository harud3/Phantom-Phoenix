using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance { get; private set; }
    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private HeroController playerHeroController, enemyHeroController;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #region 5��X�L��
    public bool IsFast(CardModel model) //����
    {
        if (model.skill1 == CardEntity.Skill.fast || model.skill2 == CardEntity.Skill.fast || model.skill3 == CardEntity.Skill.fast)
        {
            return true;
        }
        return false;
    }
    public bool IsTaunt(CardModel model) //����
    {
        if (model.skill1 == CardEntity.Skill.taunt || model.skill2 == CardEntity.Skill.taunt || model.skill3 == CardEntity.Skill.taunt)
        {
            //�G�Ȃ�7,8,9���O��ƂȂ�A�����Ȃ�1,2,3���O��ƂȂ�
            if (!model.isPlayerCard && (model.thisFieldID == 7 || model.thisFieldID == 8 || model.thisFieldID == 9)
                || (model.isPlayerCard && (model.thisFieldID == 1 || model.thisFieldID == 2 || model.thisFieldID == 3))
                )
            {
                return true;
            }
        }
        return false;
    }
    public bool IsSnipe(CardModel model) //�_��
    {
        if (!model.isSeal && (model.skill1 == CardEntity.Skill.snipe || model.skill2 == CardEntity.Skill.snipe || model.skill3 == CardEntity.Skill.snipe))
        {
            return true;
        }
        return false;
    }
    public bool IsPierce(CardModel model) //�ђ�
    {
        if (!model.isSeal &&  (model.skill1 == CardEntity.Skill.pierce || model.skill2 == CardEntity.Skill.pierce || model.skill3 == CardEntity.Skill.pierce))
        {
            return true;
        }
        return false;
    }
    public bool IsActiveDoubleAction(CardModel model) //�A����
    {
        if (!model.isSeal &&  IsDoubleAction(model))
        {
            if (model.isActiveDoubleAction) { return true; }
        }
        return false;
    }
    public bool IsDoubleAction(CardModel model) //�A��
    {
        if (!model.isSeal && ( model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction ))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// ����p�@�A�����ʂ������Ă���ƁA�������ƃ^�[���J�n���ɘA������true�ƂȂ�  �܂�A�A�����ʎ����ŘA������false�̎��́A1��ڂ̍U�����ς܂��Ă���    ����āA���̎��͍s���ł��Ȃ��悤�ɂ�����
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
    /// ���炩�̒��������݂��Ă��邩
    /// </summary>
    /// <param name="isPlayerField"></param>
    /// <returns></returns>
    public bool IsAnyTaunt(bool isPlayerField)
    {

        if (isPlayerField)
        {
            //playerFields��fieldID1,2,3�̂����A�J�[�h���R�Â��Ă���t�B�[���h�𒊏o���A���̃J�[�h�Q�̒��� isTaunt == true �Ȃ��̂�����Ȃ�Atrue��Ԃ�
            if (playerFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.isTaunt).Count() > 0) { return true; }
        }
        else
        {
            //enemyFields��fieldID1,2,3�̂����A�J�[�h���R�Â��Ă���t�B�[���h�𒊏o���A���̃J�[�h�Q�̒��� isTaunt == true �Ȃ��̂�����Ȃ�Atrue��Ԃ�
            if (enemyFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.isTaunt).Count() > 0) { return true; }
        }
        return false;
    }
    /// <summary>
    /// �u���b�N����Ă��邩
    /// </summary>
    /// <param name="isPlayerField"></param>
    /// <param name="thisFieldID"></param>
    /// <returns></returns>
    public bool IsBlock(bool isPlayerField, int thisFieldID)
    {
        //fieldID�́A�@
        //             ���O��    �O����
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //�ƂȂ��Ă���
        if (isPlayerField)
        {
            //�u���b�N
            //GetUnitByFieldID()�Ŏ擾�������j�b�g��null�ł͂Ȃ��Ȃ�A�����O�Ƀ��j�b�g�����邽�߃u���b�N���������Ă���
            if ((thisFieldID == 4 && FieldManager.instance.GetUnitByFieldID(1) != null)
                || (thisFieldID == 5 && FieldManager.instance.GetUnitByFieldID(2) != null)
                || (thisFieldID == 6 && FieldManager.instance.GetUnitByFieldID(3) != null)
                ) { return true; }
        }
        else
        {
            //�u���b�N
            if ((thisFieldID == 10 && FieldManager.instance.GetUnitByFieldID(7) != null)
                || (thisFieldID == 11 && FieldManager.instance.GetUnitByFieldID(8) != null)
                || (thisFieldID == 12 && FieldManager.instance.GetUnitByFieldID(9) != null)
                ) { return true; }
        }

        return false;
    }
    /// <summary>
    /// �E�H�[���ƂȂ��Ă��邩
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
    /// �ђʌ��ʂ̔���
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
        }�@//�O���fieldID��+3������A����fieldID�ɂȂ�
    }
    /// <summary>
    /// �e��J�[�h�̓�����ʂ𗅗񂷂�
    /// </summary>
    /// <param name="c"></param>
    /// <param name="targets"></param>
    public void SpecialSkills(CardController c, CardController[] targets = null)
    {
        HeroController h = c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        switch (c.model.cardID)
        {
            ///fire �X�y���̏�����
            //case 10:
            //    {
            //        c.hcSpellContents = (HeroController hc) => { hc.Damage(3); };
            //        c.ccSpellContents = (CardController cc) => { cc.Damage(3); };
            //        break;
            //    }

            //122
            case 1: { break; }
            //103
            case 2: { break; }
            //121
            case 3: { break; }
            //Dwarf
            case 4: { break; }
            //Behemoth
            case 5:
                {
                    h.ChangeMaxMP(-1);
                    break;
                }
            //223
            case 6: { break; }
            //232
            case 7: { break; }
            //Cerberus
            case 8: { break; }
            //321
            case 9:
                {
                    GameManager.instance.GivesCard(h.model.isPlayer, 2);
                    GameManager.instance.GivesCard(!h.model.isPlayer, 2);
                    break;
                }
            //333
            case 10:
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
            //323
            case 11:
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
            case 12:
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
            case 13: { break; }
            //443
            case 14:
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
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
            case 15:
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
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

        }

    }
}
