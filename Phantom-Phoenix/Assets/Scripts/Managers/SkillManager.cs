using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    #region �J�[�h���\
    public bool isFast(CardModel model)
    {
        if (model.skill1 == CardEntity.Skill.fast || model.skill2 == CardEntity.Skill.fast || model.skill3 == CardEntity.Skill.fast)
        {
            return true;
        }
        return false;
    }
    public bool isTaunt(CardModel model, bool isPlayerField)
    {
        if (model.skill1 == CardEntity.Skill.taunt || model.skill2 == CardEntity.Skill.taunt || model.skill3 == CardEntity.Skill.taunt)
        {
            if (!isPlayerField && (model.fieldID == 7 || model.fieldID == 8 || model.fieldID == 9)
                || (isPlayerField && (model.fieldID == 1 || model.fieldID == 2 || model.fieldID == 3))
                )
            {
                return true;
            }
        }
        return false;
    }
    public bool isSnipe(CardModel model)
    {
        if (model.skill1 == CardEntity.Skill.snipe || model.skill2 == CardEntity.Skill.snipe || model.skill3 == CardEntity.Skill.snipe)
        {
            return true;
        }
        return false;
    }
    public bool isPierce(CardModel model)
    {
        if (model.skill1 == CardEntity.Skill.pierce || model.skill2 == CardEntity.Skill.pierce || model.skill3 == CardEntity.Skill.pierce)
        {
            return true;
        }
        return false;
    }
    public bool isActiveDoubleAction(CardModel model)
    {
        if (isDoubleAction(model))
        {
            if (model.isActiveDoubleAction) { return true; }
        }
        return false;
    }
    public bool isDoubleAction(CardModel model)
    {
        if (model.skill1 == CardEntity.Skill.doubleAction || model.skill2 == CardEntity.Skill.doubleAction || model.skill3 == CardEntity.Skill.doubleAction)
        {
            return true;
        }
        return false;
    }
    #endregion
    #region �Ֆʎ擾
    /// <summary>
    /// fieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    private CardController GetCardByFieldID(int fieldID)
    {
        if (1 <= fieldID && fieldID <= 6)
        {
            if (playerFields[fieldID - 1].childCount != 0)
            {
                return playerFields[fieldID - 1].GetComponentInChildren<CardController>(); ;
            }
        }
        else if (fieldID <= 12)
        {
            if (enemyFields[fieldID - 7].childCount != 0)
            {
                return enemyFields[fieldID - 7].GetComponentInChildren<CardController>(); ;
            }
        }
        return null;
    }
    /// <summary>
    /// fieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public List<CardController> GetCardsByFieldID(int[] fieldID)
    {
        return fieldID.Select(i => GetCardByFieldID(i)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldID��1�`6���w�肷��@isPlayer��true�Ȃ疡���J�[�h���@false�Ȃ�G�J�[�h��Ԃ� �܂�A���̊֐��̈���fieldID1�`6�́AfieldID7�`12�̐����𕹂�����
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    private CardController GetCardByIsPlayerAndFieldID(bool isPlayer,int fieldID)
    {
        if (isPlayer && 1 <= fieldID && fieldID <= 6)
        {
            if (playerFields[fieldID - 1].childCount != 0)
            {
                return playerFields[fieldID - 1].GetComponentInChildren<CardController>(); ;
            }
        }
        else if (!isPlayer && 1 <= fieldID && fieldID <= 6)
        {
            if (enemyFields[fieldID - 1].childCount != 0)
            {
                return enemyFields[fieldID - 1].GetComponentInChildren<CardController>(); ;
            }
        }
        return null;
    }
    /// <summary>
    /// fieldID��1�`6���w�肷��@isPlayer��true�Ȃ疡���J�[�h���@false�Ȃ�G�J�[�h��Ԃ� �܂�A���̊֐��̈���fieldID1�`6�́AfieldID7�`12�̐����𕹂�����
    /// </summary>
    /// <param name="iPfID"></param>
    /// <returns></returns>
    private List<CardController> GetCardsByIsPlayerAndFieldID((bool isPlayer, int fieldID)[] iPfID)
    {
        return iPfID.Select(i => GetCardByIsPlayerAndFieldID(i.isPlayer, i.fieldID)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    private CardController GetRandomCards(bool isPlayerField)
    {

        if (isPlayerField)
        {
            var x = playerFields.Where(i => i.childCount != 0)?.Select(i => i.GetComponentInChildren<CardController>()).ToList();
            if (!x.Any()) { return null; }
            return x?[Random.Range(0, x.Count())];
        }
        else
        {
            var x = enemyFields.Where(i => i.childCount != 0)?.Select(i => i.GetComponentInChildren<CardController>()).ToList();
            if (!x.Any()) { return null; }
            return x?[Random.Range(0, x.Count())];
        }
    }
    public bool CheckCanAttackUnit(CardController attacker, CardController target)
    {
        //�u���b�N�⒧��������Ă���Ȃ���S�������\���Ŕ��肷�� �X�}�[�g�ȕ��@���v�����Ȃ������c
        //���̃X�N���v�g���A�^�b�`����Ă���J�[�h�̐e(�܂�u����Ă���field)��fieldID���擾����
        //fieldID�́A�@
        //             ���O��    �O����
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //�ƂȂ��Ă���
        if (target.model.isTaunt   /*isTaunt��field1,2,3�܂���field7,8,9�ɂ��鎞��true�ƂȂ�@����āAtarget��isTaunt���Ă�Ȃ瑦�J���OK */
            || isSnipe(attacker.model)) //isSnipe�͂ǂ��ł��U���ł���
        {
            return true;
        }

        if (target.model.isPlayerCard) //target��playerCard�Ȃ�A�U���Ώۑ���field��player��
        {
            if (isAnyTaunt(true)) { return false; }

            if (isBlock(true, target.model.fieldID)) { return false; }
        }
        else //����ȊO��field��enemy��
        {
            if (isAnyTaunt(false)) { return false; }

            if (isBlock(false, target.model.fieldID)) { return false; }

        }
        return true;
    }
    public bool CheckCanAttackHero(CardController attacker, HeroController target)
    {
        //�u���b�N�⒧��������Ă���Ȃ���S�������\���Ŕ��肷�� �X�}�[�g�ȕ��@���v�����Ȃ������c
        //���̃X�N���v�g���A�^�b�`����Ă���J�[�h�̐e(�܂�u����Ă���field)��fieldID���擾����
        //fieldID�́A�@
        //             ���O��    �O����
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //�ƂȂ��Ă���
        if (isSnipe(attacker.model)) //isSnipe�͂ǂ��ł��U���ł���
        {
            return true;
        }

        if (target.model.isPlayer) //hero��player�Ȃ�A�m�F����field��player��
        {
            if (isAnyTaunt(true)) { return false; }

            //�E�H�[��
            if (isWall(true)) { return false; }
        }
        else //����ȊO��enemy��
        {
            if (isAnyTaunt(false)) { return false; }

            //�E�H�[��
            if (isWall(false)) { return false; }
        }
        return true;
    }
    public bool isAnyTaunt(bool isPlayerField)
    {

        if (isPlayerField)
        {
            //playerFields��fieldID1,2,3�̂����A�J�[�h���R�Â��Ă���field�𒊏o���A���̃J�[�h�Q�̒��� isTaunt == true �Ȃ��̂�����Ȃ�Atrue��Ԃ�
            if (playerFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.isTaunt).Count() > 0) { return true; }
        }
        else
        {
            //enemyFields��fieldID1,2,3�̂����A�J�[�h���R�Â��Ă���field�𒊏o���A���̃J�[�h�Q�̒��� isTaunt == true �Ȃ��̂�����Ȃ�Atrue��Ԃ�
            if (enemyFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.isTaunt).Count() > 0) { return true; }
        }
        return false;
    }
    public bool isBlock(bool isPlayerField, int thisFieldID)
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
            //card��null�ȊO�Ȃ�A�����O�Ƀ��j�b�g�����邽�߃u���b�N���������Ă���
            if ((thisFieldID == 4 && GetCardByFieldID(1) != null)
                || (thisFieldID == 5 && GetCardByFieldID(2) != null)
                || (thisFieldID == 6 && GetCardByFieldID(3) != null)
                ) { return true; }
        }
        else
        {
            //�u���b�N
            if ((thisFieldID == 10 && GetCardByFieldID(7) != null)
                || (thisFieldID == 11 && GetCardByFieldID(8) != null)
                || (thisFieldID == 12 && GetCardByFieldID(9) != null)
                ) { return true; }
        }

        return false;
    }
    public bool isWall(bool isPlayerField)
    {
        if (isPlayerField
            && (GetCardByFieldID(1) != null || GetCardByFieldID(4) != null)
            && (GetCardByFieldID(2) != null || GetCardByFieldID(5) != null)
            && (GetCardByFieldID(3) != null || GetCardByFieldID(6) != null)
            )
        {
            return true;
        }
        else if (!isPlayerField
            && (GetCardByFieldID(7) != null || GetCardByFieldID(10) != null)
            && (GetCardByFieldID(8) != null || GetCardByFieldID(11) != null)
            && (GetCardByFieldID(9) != null || GetCardByFieldID(12) != null)
            )
        {
            return true;
        }
        return false;
    }
    public void ExecutePierce(CardController attacker, CardController target)
    {

        int targetFieldID = target.model.fieldID;
        if (isPierce(attacker.model) 
            && ( 
                (1 <= targetFieldID &&  targetFieldID <= 3) 
                || (7 <= targetFieldID && targetFieldID <= 9 ) 
            )
            ) { GetCardByFieldID(target.model.fieldID + 3)?.Damage(attacker.model.atk); }
    }
    #endregion
    public void specialSkills(CardController c, CardController[] targets = null)
    {
        HeroController h = c.model.isPlayerCard ? playerHeroController : enemyHeroController;
        switch (c.model.cardID)
        {
            ///fire
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
                    GameManager.instance.GiveCard(h.model.isPlayer, 2);
                    GameManager.instance.GiveCard(!h.model.isPlayer, 2);
                    break;
                }
            //333
            case 10:
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) => { if (isPlayerTurn == c.model.isPlayerCard) { c.Heal(1); } };
                    break;
                }
            //323
            case 11:
                {
                    c.SpecialSkillEndTurn = (bool isPlayerTurn) => { if (isPlayerTurn == c.model.isPlayerCard) { GetRandomCards(!c.model.isPlayerCard)?.Damage(1); } };
                    break;
                }
            //FireLord
            case 12:
                {
                    GetCardsByFieldID(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }).Where(i => i.model.fieldID != c.model.fieldID).ToList().ForEach(i => i.Damage(2));
                    c.SpecialSkillBeforeDie = () => { GetCardsByFieldID(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 })
                        .Where(i => i.model.fieldID != c.model.fieldID).ToList().ForEach(i => i.Damage(2)); };
                    break;
                }
            case 13: { break; }
            case 14:
                {
                    if (targets != null)
                    {
                        targets.First().Damage(2);
                    }
                    break;
                }

        }

    }
}
