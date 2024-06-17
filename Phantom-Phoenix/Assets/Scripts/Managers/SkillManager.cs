using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
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
        if (model.skill1 == CardEntity.skill.fast || model.skill2 == CardEntity.skill.fast || model.skill3 == CardEntity.skill.fast)
        {
            return true;
        }
        return false;
    }
    public bool isTaunt(CardModel model, bool isPlayerField)
    {
        if (model.skill1 == CardEntity.skill.taunt || model.skill2 == CardEntity.skill.taunt || model.skill3 == CardEntity.skill.taunt)
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
        if (model.skill1 == CardEntity.skill.snipe || model.skill2 == CardEntity.skill.snipe || model.skill3 == CardEntity.skill.snipe)
        {
            return true;
        }
        return false;
    }
    public bool isPierce(CardModel model)
    {
        if (model.skill1 == CardEntity.skill.pierce || model.skill2 == CardEntity.skill.pierce || model.skill3 == CardEntity.skill.pierce)
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
        if (model.skill1 == CardEntity.skill.doubleAction || model.skill2 == CardEntity.skill.doubleAction || model.skill3 == CardEntity.skill.doubleAction)
        {
            return true;
        }
        return false;
    }
    #endregion
    #region �Ֆʎ擾
    /// <summary>
    /// FieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
    /// <param name="FieldID"></param>
    /// <returns></returns>
    private CardController GetCardbyFieldID(int FieldID)
    {
        if (1 <= FieldID && FieldID <= 6)
        {
            if (playerFields[FieldID - 1].childCount != 0)
            {
                return playerFields[FieldID - 1].GetComponentInChildren<CardController>(); ;
            }
        }
        else if (FieldID <= 12)
        {
            if (enemyFields[FieldID - 7].childCount != 0)
            {
                return enemyFields[FieldID - 7].GetComponentInChildren<CardController>(); ;
            }
        }
        return null;
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
            if ((thisFieldID == 4 && GetCardbyFieldID(1) != null)
                || (thisFieldID == 5 && GetCardbyFieldID(2) != null)
                || (thisFieldID == 6 && GetCardbyFieldID(3) != null)
                ) { return true; }
        }
        else
        {
            //�u���b�N
            if ((thisFieldID == 10 && GetCardbyFieldID(7) != null)
                || (thisFieldID == 11 && GetCardbyFieldID(8) != null)
                || (thisFieldID == 12 && GetCardbyFieldID(9) != null)
                ) { return true; }
        }

        return false;
    }
    public bool isWall(bool isPlayerField)
    {
        if (isPlayerField
            && (GetCardbyFieldID(1) != null || GetCardbyFieldID(4) != null)
            && (GetCardbyFieldID(2) != null || GetCardbyFieldID(5) != null)
            && (GetCardbyFieldID(3) != null || GetCardbyFieldID(6) != null)
            )
        {
            return true;
        }
        else if (!isPlayerField
            && (GetCardbyFieldID(7) != null || GetCardbyFieldID(10) != null)
            && (GetCardbyFieldID(8) != null || GetCardbyFieldID(11) != null)
            && (GetCardbyFieldID(9) != null || GetCardbyFieldID(12) != null)
            )
        {
            return true;
        }
        return false;
    }
    public void DealAnySkillByAttack(CardController attacker, CardController target)
    {

        int targetFieldID = target.model.fieldID;
        if (isPierce(attacker.model) 
            && ( 
                (1 <= targetFieldID &&  targetFieldID <= 3) 
                || (7 <= targetFieldID && targetFieldID <= 9 ) 
            )
            ) { GetCardbyFieldID(target.model.fieldID + 3)?.Damage(attacker.model.atk); }
    }
    #endregion
    public void specialSkills(CardController cc)
    {
        HeroController hero = cc.model.isPlayerCard ? playerHeroController : enemyHeroController;
        switch (cc.model.cardID)
        {
            case 1:
                {
                    break;
                }
            case 2:
                {
                    break;
                }
            case 3:
                {
                    break;
                }
            case 4:
                {
                    //cc.SpecialSkillAfterAttack = (isAttacker) => { if (isAttacker) { hero.ChangeMaxMP(-1); } };
                    break;
                }
            case 5:
                {
                    hero.ChangeMaxMP(-1);
                    break;
                }
        }
    }
}
