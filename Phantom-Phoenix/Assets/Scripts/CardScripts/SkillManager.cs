using Photon.Pun.Demo.Cockpit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

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
        FieldManager.instance.GetUnitsByFieldID(fieldsID)?.ForEach(i => i.TensionSkill?.Invoke());
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
    public bool IsTaunt(CardModel model) //����
    {
        if (model.skill1 == CardEntity.Skill.taunt || model.skill2 == CardEntity.Skill.taunt || model.skill3 == CardEntity.Skill.taunt || model.skill4 == CardEntity.Skill.taunt || model.skill5 == CardEntity.Skill.taunt)
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
    /// �e��J�[�h�̓�����ʂ𗅗񂷂�
    /// </summary>
    /// <param name="c"></param>
    /// <param name="targets"></param>
    public void SpecialSkills(CardController c, CardController[] targets = null)
    {
        HeroController h = c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        TensionController t = c.model.isPlayerCard ? playerTensionController : enemyTensionController;
        switch (c.model.cardID)
        {
            ///fire �X�y���̏�����
            //case 10:
            //    {
            //        c.hcSpellContents = (HeroController hc) => { hc.Damage(3); };
            //        c.ccSpellContents = (CardController cc) => { cc.Damage(3); };
            //        break;
            //    }

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
                    GameManager.instance.GivesCard(h.model.isPlayer, 2);
                    GameManager.instance.GivesCard(!h.model.isPlayer, 2);
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
            case 12: //�����^�[���I����:�����_���ȃ��j�b�g1�̂�1�_���[�W
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
                        if (isAttacker) { GameManager.instance.GivesCard(h.model.isPlayer, 1); }
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
                    FieldManager.instance.GetUnitsInHand(c.model.isPlayerCard)?.Where(i => i.model.defaultATK <= 2).ToList().ForEach(i => i.SilentBuff(1, 1));
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

                void SummonTelf111()
                {
                    if (FieldManager.instance.GetEmptyFieldID(c.model.isPlayerCard) is var x && x.emptyField != null)
                    {
                        CardController cc = Instantiate(cardPrefab, x.emptyField);
                        cc.Init(10002, c.model.isPlayerCard); // cardID10002 = telf111;
                        cc.SummonOnField(x.fieldID, ExecuteReduceMP: false);
                    }
                }
            //uelf101 
            case 33: //���S��:telf111���o��
                {

                    c.SpecialSkillBeforeDie = () =>
                    {
                        CardController cc = Instantiate(cardPrefab, c.transform.parent);
                        cc.Init(10002, c.model.isPlayerCard);
                        cc.SummonOnField(c.model.thisFieldID, ExecuteReduceMP: false);
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
                    if(FieldManager.instance.GetUnitsByIsPlayer(c.model.isPlayerCard) is var x && x.Where(i => i.model.cost <= 1)?.Count() != 0)
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
                            SummonTelf111();
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
                        GameManager.instance.GivesCard(c.model.isPlayerCard, 1);
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

        }
    }
}
