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
    #region 5��X�L��
    public bool IsFast(CardModel model) //����
    {
        if (model.skill1 == CardEntity.Skill.fast || model.skill2 == CardEntity.Skill.fast || model.skill3 == CardEntity.Skill.fast || model.skill4 == CardEntity.Skill.fast || model.skill5 == CardEntity.Skill.fast)
        {
            return true;
        }
        return false;
    }
    public bool hasTaunt(CardModel model)
    {
        if (model.skill1 == CardEntity.Skill.taunt || model.skill2 == CardEntity.Skill.taunt || model.skill3 == CardEntity.Skill.taunt || model.skill4 == CardEntity.Skill.taunt || model.skill5 == CardEntity.Skill.taunt)
        {
            return true;
        }
        return false;
    }
    public bool IsTaunt(CardModel model) //����
    {
        if (hasTaunt(model))
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
        if (!model.isSeal && (model.skill1 == CardEntity.Skill.snipe || model.skill2 == CardEntity.Skill.snipe || model.skill3 == CardEntity.Skill.snipe) || model.skill4 == CardEntity.Skill.snipe || model.skill5 == CardEntity.Skill.snipe)
        {
            return true;
        }
        return false;
    }
    public bool IsPierce(CardModel model) //�ђ�
    {
        if (!model.isSeal &&  (model.skill1 == CardEntity.Skill.pierce || model.skill2 == CardEntity.Skill.pierce || model.skill3 == CardEntity.Skill.pierce) || model.skill4 == CardEntity.Skill.pierce || model.skill5 == CardEntity.Skill.pierce)
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
        if (!model.isSeal && ( model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction || model.skill4 == CardEntity.Skill.doubleAction || model.skill5 == CardEntity.Skill.doubleAction))
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
        if (!model.isActiveDoubleAction && (model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction || model.skill4 == CardEntity.Skill.doubleAction || model.skill5 == CardEntity.Skill.doubleAction))
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
    /// �e���V�����ђʌ��ʂ̔���
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
        }�@//�O���fieldID��+3������A����fieldID�ɂȂ�
    }
    /// <summary>
    /// �e��J�[�h�̓�����ʂ𗅗񂷂�
    /// </summary>
    /// <param name="c"></param>
    /// <param name="targets"></param>
    public void SpecialSkills(CardController c, CardController[] targets = null, HeroController hctarget = null)
    {
        HeroController h = c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        HeroController eh = !c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        TensionController t = c.model.isPlayerCard ? playerTensionController : enemyTensionController;
        TensionController et = c.model.isPlayerCard ? playerTensionController : enemyTensionController;

        switch (c.model.cardID)
        {
            //011
            case 1: { break; } //�Ȃ�
            //122
            case 2: { break; } //�Ȃ�
            //102
            case 3: { break; } //����
            //121
            case 4: { break; } //�_��
            //Dwarf
            case 5: { break; } //�����_���A��
            //Behemoth
            case 6: //������:�����q�[���[��MP-1
                {
                    h.ChangeMaxMP(-1);
                    break;
                }
            //222
            case 7: { break; } //����
            //232
            case 8: { break; } //�ђ�
            //Cerberus
            case 9: { break; } //����
            //321
            case 10: //���q�[���[�̓J�[�h��2������
                {
                    GameManager.instance.GiveCards(h.model.isPlayer, 2);
                    GameManager.instance.GiveCards(!h.model.isPlayer, 2);
                    break;
                }
            //333 
            case 11: //�����^�[���I����:HP1��
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
            case 12: //�����^�[���I����:�����_���ȓG���j�b�g1�̂�1�_���[�W
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
            case 13: //������&���S��:�S�Ẵ��j�b�g��1�_���[�W
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
            //445
            case 14: { break; }�@//�Ȃ�
            //443
            case 15: //������:�G���j�b�g1�̂�2�_���[�W
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
            case 16: //������:���j�b�g1�̂𕕈󂷂�
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
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
            case 17: { h.Heal(5); break; } //������:�����q�[���[��HP5��
            //501
            case 18: //���S��:�S�Ă̓G���j�b�g��R�Ă�����
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(7, 6).ToArray() : Enumerable.Range(1, 6).ToArray()
                            ).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            //�Ώۃ��j�b�g�̃^�[���I��������X�L����ǉ�����
                            x.ForEach(i =>
                            {
                                i.SetIsBurning(true);
                                i.SpecialSkillEndTurn += (bool isPlayerTurn) => { i.Damage(1); };
                            });
                        }
                    };
                    break;
                }
            //517
            case 19: //�ђ� �U����:�����q�[���[�̓J�[�h��1������
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        if (isAttacker) { GameManager.instance.GiveCards(h.model.isPlayer, 1); }
                    };
                    break;
                }
            //Driller
            case 20: //������:���̃o�g�����A����ATK��2�ȉ��̖������j�b�g��+1/+1
                {
                    //Driller��2/2�Ȃ̂őΏ�
                    c.Buff(1, 1);
                    //�t�B�[���h�̑Ώۃ��j�b�g��+1/+1
                    FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(1, 6).ToArray() : Enumerable.Range(7, 6).ToArray()
                            ).Where(i => i.model.defaultATK <= 2 && i.model.thisFieldID != c.model.thisFieldID).ToList()?.ForEach(i => i.Buff(1, 1));
                    //��D�̑ΏۃJ�[�h��+1/+1
                    FieldManager.instance.GetCardsInHand(c.model.isPlayerCard)?.Where(i => i.model.defaultATK <= 2).ToList().ForEach(i => i.SilentBuff(1, 1));
                    //��������J�[�h��+1/+1����悤��
                    h.ccExternalBuff += (CardController cc) => { if (cc.model.defaultATK <= 2) { cc.SilentBuff(1, 1); } };
                    break;
                }
            //656
            case 21: //�_��
                { break; }
            case 22: //������&�����^�[���I����: �����t�B�[���h�ɁAcardID1��0/1/1��1�̏o��
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
            //633
            case 23: { break; } //���� �����t�B�[���h��\n���j�b�g�̐����A�R�X�g-1
            //777
            case 24: { break; } //����
            //746
            case 25: //���j�b�g��I���@�����Ȃ�4�� �G�Ȃ�4�_���[�W
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
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
            case 26: //���� �ђ� �U����:���S���� �O�񎀖S��:���ɕ������� ��񎀖S��:�O��ɕ������� 
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
            //unit877
            case 27: //ATK��1�`13�̃����_���Ȓl�ɂ��āAHP��ATK-13�̒l�ɂ���
                {
                    var i = Random.Range(1, 14);
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
            //unit863
            case 28: //�����_���Ȗ������j�b�g1�̂����S��������A�����_���ȓG���j�b�g1�̂����S������
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
            case 29: //���S��:�S�Ă̖������j�b�g�ɑ_����t�^
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        var x = FieldManager.instance.GetUnitsByFieldID(
                            c.model.isPlayerCard ? Enumerable.Range(1, 6).ToArray() : Enumerable.Range(7, 6).ToArray()
                            ).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                        if (x.Count != 0)
                        {
                            //�Ώۃ��j�b�g�ɑ_����t�^����
                            x.ForEach(i =>
                            {
                                i.SetIsSnipe(true);
                            });
                        }
                    };
                    break;
                }
            //unit925
            case 30: //������:�S�Ẵ��j�b�g��4�_���[�W
                {
                    var x = FieldManager.instance.GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray()).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList();
                    if (x.Count != 0)
                    {
                        x.ForEach(i => i.Damage(4));
                    }
                    break;
                }
            //unit945
            case 31: //������:cardID3,4,7,8���o��
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
            case 32: //����+2/+2 �G-2/-2
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

                void SummonTelf111(int avoidFieldID = 99)
                {
                    if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard, avoidFieldID) is var x && x.emptyField != null)
                    {
                        CardController cc = Instantiate(cardPrefab, x.emptyField);
                        cc.Init(10002, c.model.isPlayerCard); // cardID10002 = telf111;
                        cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                    }
                }
                void SummonTelf111ByFieldID(Transform field, int fieldID, bool changePlayerCard = false)
                {
                    CardController cc = Instantiate(cardPrefab, field);
                    cc.Init(10002, changePlayerCard ? !c.model.isPlayerCard : c.model.isPlayerCard);
                    cc.SummonOnField(fieldID, ExecuteReduceMP: false);
                }
            //uelf101 
            case 33: //���S��:telf111���o��
                {

                    c.SpecialSkillBeforeDie = () =>
                    {
                        SummonTelf111ByFieldID(c.transform.parent, c.model.thisFieldID);
                    };
                    break;
                }
            //uelf221
            case 34: //�e���V�����㏸��:�����t�B�[���h��telf111���o��
                {

                    c.TensionSkill = () =>
                    {
                        SummonTelf111();
                    };
                    break;
                }
            //uelf212
            case 35: //������:1�R�X�g�ȉ��̖������j�b�g������Ȃ�+1/+1
                {
                    if(FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1)?.Count() != 0)
                    {
                        c.Buff(1, 1);
                    }
                    break;
                }
            //uelf222
            case 36: //�U����:�����t�B�[���h��telf111���o��
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        if (isAttacker)
                        {
                            SummonTelf111(c.model.thisFieldID);
                        }
                    };
                    break;
                }
            //self2
            case 37: //�e���V����+1 �J�[�h��1������
                {
                    c.SpellContents = () =>
                    {
                        t.SetTension(t.model.tension + 1);
                        GameManager.instance.GiveCards(c.model.isPlayerCard, 1);
                    };
                    break;
                }
            //uelf312
            case 38: //���̃��j�b�g�̏㉺��telf111���o��
                {
                    FieldManager.instance.GetEmptyUpDownFieldID(c.model.thisFieldID)?.Where(i => i.emptyField != null).ToList().ForEach(i => SummonTelf111ByFieldID(i.emptyField, i.fieldID));
                    break;
                }
            //unit301
            case 39: //1�R�X�g�ȉ��̖������j�b�g�����S�����A���̐���+1/+1
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        x.Where(i => i.model.defaultCost <= 1).ToList().ForEach(i =>
                        {
                            i.Damage(99);
                            c.Buff(1, 1);
                        });
                    }
                    break;
                }
            //self3
            case 40: //�G���j�b�g1�̂�0�_���[�W�@1�R�X�g�ȉ��̖������j�b�g�̐����_���[�W+1
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
            case 41: //1�R�X�g�ȉ��̖������j�b�g�̐���ATK+1
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1).Count() is var cnt && cnt >= 1)
                    {
                        c.Buff(cnt, 0);
                    }
                    break;
                }
            //guardb
            case 42: //1�R�X�g�ȉ��̖������j�b�g�̐���HP+1
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.defaultCost <= 1).Count() is var cnt && cnt >= 1)
                    {
                        c.Buff(0, cnt);
                    }
                    break;
                }
            //uelf423
            case 43: //�����q�[���[��HP��1�� 1�R�X�g�ȉ��̖������j�b�g�̐����A�񕜗�+1
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
            case 44: //��U����:telf111���o��
                {
                    c.SpecialSkillBeforeAttack = (bool isAttacker) =>
                    {
                        SummonTelf111(c.model.thisFieldID);
                    };
                    break;
                }
            //unit433
            case 45: //1�R�X�g�ȉ��̖������j�b�g�����S�����A���̐����J�[�h������
                {
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        var y = x.Where(i => i.model.defaultCost <= 1);
                        y.ToList().ForEach(i =>
                        {
                            i.Damage(99);
                        });
                        if(y.Count() is var i && i > 0)GameManager.instance.GiveCards(c.model.isPlayerCard, i);
                        
                    }
                    break;
                }
            //self4
            case 46: //�S�Ă̓G���j�b�g��1�_���[�W telf111��3�̏o��
                {
                    c.SpellContents = () =>
                    {
                        if (FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard) is var x && x != null)
                        {
                            x.ForEach(i => i.DamageFromSpell(1, c.model.isPlayerCard));
                        }
                        SummonTelf111();
                        SummonTelf111();
                        SummonTelf111();
                    };
                    break;
                }
            //uelf542
            case 47: //�S�Ẵt�B�[���h��telf111���o��
                {
                    FieldManager.instance.GetEmptyFieldIDs(c.model.isPlayerCard).ForEach(i => SummonTelf111ByFieldID(i.emptyField, i.fieldID)); //�����t�B�[���h
                    FieldManager.instance.GetEmptyFieldIDs(!c.model.isPlayerCard).ForEach(i => SummonTelf111ByFieldID(i.emptyField, i.fieldID, true)); //�G�t�B�[���h
                    break;
                }
            //uelf525
            case 48: //�����^�[���I����:�O��ɂ���Ȃ�telf111��2�̏o��
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) =>
                    {
                        if (isPlayerTurn == c.model.isPlayerCard)
                        {
                            if(c.model.thisFieldID % 6 == 1 || c.model.thisFieldID % 6 == 2 || c.model.thisFieldID % 6 == 3)
                            {
                                SummonTelf111();
                                SummonTelf111();
                            }
                        }
                    };
                    break;
                }
            //uelf622
            case 49: //�G1�̂�0�_���[�W 1�R�X�g�ȉ��̖������j�b�g�̐����_���[�W+1
                {
                    var plusDamage = 0;
                    if (FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x != null)
                    {
                        plusDamage = x.Where(i => i.model.defaultCost <= 1).Count();
                    }

                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
                    {
                        var y = FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard);
                        if (y != null)
                        {
                            y.Damage(plusDamage);
                        }
                        else
                        {
                            playerHeroController.Damage(plusDamage);
                        }
                    }
                    else if (targets != null)
                    {
                        targets.First().Damage(plusDamage);
                    }
                    else if(hctarget != null)
                    {
                        hctarget.Damage(plusDamage);
                    }
                    break;

                }
            //uelf633
            case 50: //�e���V�����㏸��:�����t�B�[���h��telf111��2�̏o��
                {

                    c.TensionSkill = () =>
                    {
                        SummonTelf111(); SummonTelf111();
                    };
                    break;
                }
            //uelf711
            case 51: { break; } //���̑ΐ풆�Ɏ��S����1�R�X�g�ȉ��̖������j�b�g�̐���+1/+1
            //uelf955
            case 52: { break; } //�����t�B�[���h��\n1�R�X�g�ȉ��̖������j�b�g�̐����A�R�X�g-1
            //switch0
            case 53: //1�_���[�W�@�����q�[���[�ɌŒ�1�_���[�W
            {
                c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(1, c.model.isPlayerCard); h.Damage(1); };
                c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(1, c.model.isPlayerCard); h.Damage(1); };
                break;
            }
            //switch1
            case 54: //�S�Ă̓G���j�b�g��1�_���[�W�@�����q�[���[�ɌŒ�1�_���[�W
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DamageFromSpell(1, c.model.isPlayerCard));
                        h.Damage(1);
                    };
                    break;
                }
            //uwitch221
            case 55: //���S��:�f�b�L����X�y����1������
                {
                    c.SpecialSkillBeforeDie = () =>
                    {
                        GameManager.instance.GiveSearchCards(c.model.isPlayerCard, 1, (i) => { return GameDataManager.instance.cardlist.cl[i - 1].category == Category.spell; });
                        
                    };
                    break;
                }
            //uwitch223
            case 56: //�X�y���g�p��:ATK+1
                {
                    c.SpellUsedSkill = () =>
                    {
                        c.Buff(1, 0);
                    };
                    break;
                }
            //switch2d
            case 57: //2�_���[�W�@�����q�[���[�ɌŒ�1�_���[�W
                {
                    c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(2, c.model.isPlayerCard); h.Damage(1); };
                    c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(2, c.model.isPlayerCard); h.Damage(1); };
                    break;
                }
            //switch2h
            case 58: //3�񕜁@�J�[�h��1������
                {
                    c.hcSpellContents = (HeroController hc) => { hc.Heal(3); GameManager.instance.GiveCards(c.model.isPlayerCard, 1); };
                    c.ccSpellContents = (CardController cc) => { cc.Heal(3); GameManager.instance.GiveCards(c.model.isPlayerCard, 1); };
                    break;
                }
            //uwitch315
            case 59: //�X�y���_���[�W+1
                {
                    h.spellDamageBuff(1);
                    c.SpecialSkillBeforeDie = () =>
                    {
                        h.spellDamageBuff(-1);
                    };
                    break;
                }
            //iceMaiden
            case 60: //�����@������&���S��:�S�Ă̖�����1�񕜂���
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
            case 61: //�X�y���g�p��:HP+1
                {
                    c.SpellUsedSkill = () =>
                    {
                        c.Buff(0, 1);
                    };
                    break;
                }
            //uwitch433
            case 62: //�X�y���g�p��:�O��ɂ���Ȃ�A�e���V����+1
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
            case 63: //4�_���[�W�@�����q�[���[�ɌŒ�2�_���[�W
                {
                    c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(4, c.model.isPlayerCard); h.Damage(1); };
                    c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(4, c.model.isPlayerCard); h.Damage(1); };
                    break;
                }
            //switch4h
            case 64: //6�񕜁@�J�[�h��2������
                {
                    c.hcSpellContents = (HeroController hc) => { hc.Heal(6); GameManager.instance.GiveCards(c.model.isPlayerCard, 2); };
                    c.ccSpellContents = (CardController cc) => { cc.Heal(6); GameManager.instance.GiveCards(c.model.isPlayerCard, 2); };
                    break;
                }
            //uwitch545
            case 65: //�����^�[���I���� �����q�[���[��HP��2�񕜁@�����_���ȓG���j�b�g��1�_���[�W
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
            case 66: //�R�X�g3�ȉ��̑S�Ă̓G���j�b�g�����S������
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).Where(x => x.model.defaultCost <= 3).ToList().ForEach(i => i.Damage(99));
                    };
                    break;
                }
            //uwitch624
            case 67: //���� �_�� ���̑ΐ풆�Ɏg�p�����X�y���̐����A�R�X�g-1
                {
                    break;
                }
            //zodiac
            case 68: //���q�[���[��7�_���[�W
                {
                    h.Damage(7);
                    eh.Damage(7);
                    break;
                }
            //uwitch744
            case 69: //������:��D�̑S�ẴX�y���̃R�X�g-1
                {
                    FieldManager.instance.GetCardsInHand(c.model.isPlayerCard).Where(i => i.model.category == Category.spell).ToList().ForEach(i => i.ChangeCost(i.model.cost - 2));
                    break;
                }
            //switch7
            case 70: //�S�Ă̓G���j�b�g��5�_���[�W�@�����q�[���[�ɌŒ�3�_���[�W
                {
                    c.SpellContents = () =>
                    {
                        FieldManager.instance.GetUnitsByIsPlayer(!c.model.isPlayerCard).ForEach(i => i.DamageFromSpell(5, c.model.isPlayerCard));
                        h.Damage(3);
                    };
                    break;
                }
            //switch10
            case 71: //�S�Ă̓G��2�_���[�W�@�e���V����3�Ȃ�A�_���[�W+3&�e���V����-3
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
            case 72: //6�_���[�W �����q�[���[�ɌŒ�3�_���[�W ���̑ΐ풆�Ɏg�p�����X�y���̐����R�X�g-1
                {
                    c.hcSpellContents = (HeroController hc) => { hc.DamageFromSpell(6, c.model.isPlayerCard); h.Damage(3); };
                    c.ccSpellContents = (CardController cc) => { cc.DamageFromSpell(6, c.model.isPlayerCard); h.Damage(3); };
                    break;
                }
            void GetItemCard(int cnt)
            {
                    if(cnt <= 0) {  return; }
                    int[] cardIDs = Enumerable.Range(0, cnt).Select(i => Random.Range(10003, 10006)).ToArray();
                    GameManager.instance.GiveSpecificCards(c.model.isPlayerCard, cardIDs);
            }
            //uking111
            case 73: //������:�������j�b�g��+1/+1
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
                    {
                        FieldManager.instance.GetRandomUnits(false, c)?.Buff(1, 1);
                    }
                    targets?.First().Buff(1, 1);
                    break;
                }
            //sking1
            case 74: //�A�C�e���J�[�h��2�`3��������
                {
                    c.SpellContents = () =>
                    {
                        GetItemCard(Random.Range(2,4));
                    };
                    break;
                }
            //uking222
            case 75: //������:�A�C�e���J�[�h��1�`2��������
                {
                    GetItemCard(Random.Range(1, 3));
                    break;
                }
            //uking211
            case 76: //������:�������j�b�g�����Ȃ��Ȃ�2/1/1��2�̏o��
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
            case 77: //�����t�^ HP+2
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        cc.SetIsTaunt(true);
                        cc.Buff(0, 2);
                    };
                    break;
                }
            //sking2pierce
            case 78: //�ђʕt�^ ATK+2
                {
                    c.ccSpellContents = (CardController cc) =>
                    {
                        cc.SetIsPierce(true);
                        cc.Buff(2, 0);
                    };
                    break;
                }
            //uking312
            case 79: //�틭����:ATK+1
                {
                    c.SpecialSkillAfterBuff = () =>
                    {
                        c.Buff(1, 0, false);
                    };
                    break;
                }
            //uking322
            case 80: //������:�������j�b�g1�̂�HP+1 �����t�^
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
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
            case 81: //������&���S��&�����^�[���I����: �A�C�e���J�[�h��1��������
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
            case 82: //�e���V�����㏸��:���g�ȊO�̃����_���Ȗ������j�b�g��+1/+1
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
            case 83: //�틭����:�����_���ȓG���j�b�g��2�_���[�W
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
            case 84: //�s���ł��Ȃ� �����^�[���I����:�A�C�e���J�[�h��1��������
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
            case 85: //������:�S�Ă̖������j�b�g��+1/+1
                {
                    if(FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard).Where(i => i.model.thisFieldID != c.model.thisFieldID).ToList() is var x && x != null){
                        x.ForEach(i => i.Buff(1,1));
                    }
                    break;
                }
            //uking612
            case 86: //������:�����_���ȓG���j�b�g��1�_���[�W ���̑ΐ풆�ɃA�C�e���J�[�h�ŋ������s�����񐔕��J��Ԃ�
                {
                    for (int i = 0; i < (c.model.isPlayerCard ? FieldManager.instance.playerBuffedCntByItemCard : FieldManager.instance.enemyBuffedCntByItemCard); i++)
                    {
                        FieldManager.instance.GetRandomUnits(!c.model.isPlayerCard).Damage(1);
                    }
                    break;
                }
            //uking644
            case 87: //������:�����e���V�����̐����A�������j�b�g1�̂��o�t����
                {
                    var x = t.model.tension;
                    targets?.First().Buff(x,x);
                    break;
                }
            //uking723
            case 88: //�틭����:7/2/3���o��
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
            case 89: //���̖������j�b�g��S�Ď��S�����AHP��ATK���z������
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
                    c.Buff(plusATK, plusHP);
                    break;
                }
            //tamer
            case 90: //�������j�b�g1�̂̃X�^�b�c��2�{�ɂ���
                {
                    if (!GameDataManager.instance.isOnlineBattle && !c.model.isPlayerCard) //AI����
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
            case 91: { break; } //�A�C�e���J�[�h�ŋ������s�����񐔕��R�X�g-1
            //uking946
            case 92: //������:�������j�b�g������Ȃ�S�Ẵ��j�b�g��+2/+2 ���Ȃ��Ȃ�+5/+3
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
            //telf122
            case 10001: { break; } //����
            //telf111
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
        }

    }
    /// <summary>
    /// ��D�ɂ����Ԃł��Q�Ƃ����A�O���v���ɂ���Ĕ�������󓮓I�ȃX�L���̕R�Â�
    /// </summary>
    /// <param name="c"></param>
    public void UpdateSkills(CardController c)
    {
        switch (c.model.cardID)
        {
            //633 //���� �����t�B�[���h��\n���j�b�g�̐����A�R�X�g-1
            case 23:
                {
                    var recordCost = 6;
                    void ChangeCost()
                    {
                        if (recordCost != 6 - (c.model.isPlayerCard ? FieldManager.instance.playerFieldOnUnitCnt : FieldManager.instance.enemyFieldOnUnitCnt))
                        {
                            c.ChangeCost(6 - (c.model.isPlayerCard ? FieldManager.instance.playerFieldOnUnitCnt : FieldManager.instance.enemyFieldOnUnitCnt));
                            recordCost = c.model.cost;
                        }
                    }

                    ChangeCost();
                    c.UpdateSkill += () =>
                    {
                        ChangeCost();
                    };
                    break;
                }
            //uelf711
            case 51: //���̑ΐ풆�Ɏ��S����1�R�X�g�ȉ��̖������j�b�g�̐���+1/+1
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
            case 52: //�����t�B�[���h��\n1�R�X�g�ȉ��̖������j�b�g�̐����A�R�X�g-1
                {
                    var recordCost = 9;
                    IEnumerator ChangeCost()
                    {
                        yield return null;
                        var x = FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard).Where(i => i.model.cost <= 1).Count();
                        if (recordCost != 9 - x)
                        {
                            c.ChangeCost(9 - x);
                            recordCost = c.model.cost;
                        }
                    }

                    StartCoroutine(ChangeCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(ChangeCost());
                    };
                    break;
                }
            //uwitch624
            case 67: //���� �_�� ���̑ΐ풆�Ɏg�p�����X�y���̐����A�R�X�g-1
                {
                    var recordCost = 6;
                    IEnumerator ChangeCost()
                    {
                        yield return null;
                        var x = (c.model.isPlayerCard ? FieldManager.instance.playerUsedSpellList : FieldManager.instance.enemyUsedSpellList).Count();
                        if (recordCost != 6 - x)
                        {
                            c.ChangeCost(6 - x);
                            recordCost = c.model.cost;
                        }
                    }

                    StartCoroutine(ChangeCost());
                    c.UpdateSkill += () =>
                    {
                        StartCoroutine(ChangeCost());
                    };
                    break;
                }
            //switch20
            case 72: //6�_���[�W �����q�[���[�ɌŒ�3�_���[�W ���̑ΐ풆�Ɏg�p�����X�y���̐����R�X�g-1
                {
                    var recordSpellCount = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = (c.model.isPlayerCard ? FieldManager.instance.playerUsedSpellList : FieldManager.instance.enemyUsedSpellList).Count();
                        if (recordSpellCount != x)
                        {
                            c.CreaseCost(recordSpellCount - x);
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
            case 91: //�A�C�e���J�[�h�ŋ������s�����񐔕��R�X�g-1
                {
                    var recordBuffByItemCard = 0;
                    IEnumerator CreaseCost()
                    {
                        yield return null;
                        var x = (c.model.isPlayerCard ? FieldManager.instance.playerBuffedCntByItemCard : FieldManager.instance.enemyBuffedCntByItemCard);
                        if (recordBuffByItemCard != x)
                        {
                            c.CreaseCost(recordBuffByItemCard - x);
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
