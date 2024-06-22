using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldManager : MonoBehaviour
{
    public static FieldManager instance { get; private set; }
    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private HeroController playerHeroController, enemyHeroController;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #region �Ֆʎ擾
    //fieldID�́A�@
    //             ���O��    �O����
    //              4   1   |   7   10
    //playerHero    5   2   |   8   11  enemyHero
    //              6   3   |   9   12
    //�ƂȂ��Ă���
    /// <summary>
    /// fieldID����Y���t�B�[���h�ɂ���CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�   fieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public CardController GetUnitByFieldID(int fieldID)
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
    /// fieldID[]����Y���t�B�[���h�ɂ���CardController[]���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ��Ǝv��...     fieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsByFieldID(int[] fieldID)
    {
        return fieldID.Select(i => GetUnitByFieldID(i)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldID����Y���t�B�[���h�ɂ���CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�    fieldID��1�`6���w�肷��@isPlayer��true�Ȃ疡�����j�b�g���@false�Ȃ�G���j�b�g��Ԃ� �܂�A���̊֐��̈���fieldID1�`6�́AfieldID7�`12�̐����𕹂�����
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public CardController GetUnitByIsPlayerAndFieldID(bool isPlayer, int fieldID)
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
    /// fieldID[]����Y���t�B�[���h�ɂ���CardController[]���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ��Ǝv��...   fieldID��1�`6���w�肷��@isPlayer��true�Ȃ疡�����j�b�g���@false�Ȃ�G���j�b�g��Ԃ� �܂�A���̊֐��̈���fieldID1�`6�́AfieldID7�`12�̐����𕹂�����
    /// </summary>
    /// <param name="iPfID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsByIsPlayerAndFieldID((bool isPlayer, int fieldID)[] iPfID)
    {
        return iPfID.Select(i => GetUnitByIsPlayerAndFieldID(i.isPlayer, i.fieldID)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// �Y���t�B�[���h���ނɂ��郉���_����CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public CardController GetRandomUnits(bool isPlayerField)
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
    /// <summary>
    /// �Ώۃ��j�b�g���U���\���ǂ����𔻒肷��
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
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
        if (target.model.isTaunt   /*isTaunt��field1,2,3�܂���field7,8,9�ɂ��鎞��true�ƂȂ�ݒ�@����āAtarget��isTaunt���Ă�Ȃ瑦�J���OK */
            || SkillManager.instance.isSnipe(attacker.model)) //isSnipe�͂ǂ��ł��U���ł���
        {
            return true;
        }

        if (target.model.isPlayerCard) //target��isPlayerCard�ƁA�U������鑤�̃t�B�[���h�Q���v���C���[�̃t�B�[���h�ł��邩 �͈�v����
        {
            if (SkillManager.instance.isAnyTaunt(true)) { return false; }

            if (SkillManager.instance.isBlock(true, target.model.thisFieldID)) { return false; }
        }
        else //����ȊO��field��enemy��
        {
            if (SkillManager.instance.isAnyTaunt(false)) { return false; }

            if (SkillManager.instance.isBlock(false, target.model.thisFieldID)) { return false; }

        }
        return true;
    }
    /// <summary>
    /// �Ώۃq�[���[���U���\���ǂ������肷��
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
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
        if (SkillManager.instance.isSnipe(attacker.model)) //isSnipe�͂ǂ��ł��U���ł���
        {
            return true;
        }

        if (target.model.isPlayer) //target��isPlayer�ƁA�U������鑤�̃t�B�[���h�Q���v���C���[�̃t�B�[���h�ł��邩 �͈�v����
        {
            if (SkillManager.instance.isAnyTaunt(true)) { return false; }

            //�E�H�[��
            if (SkillManager.instance.isWall(true)) { return false; }
        }
        else //����ȊO��enemy��
        {
            if (SkillManager.instance.isAnyTaunt(false)) { return false; }

            //�E�H�[��
            if (SkillManager.instance.isWall(false)) { return false; }
        }
        return true;
    }
    #endregion
}
