using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FieldManager : MonoBehaviour
{
    public static FieldManager instance { get; private set; }
    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private Transform playerHand, enemyHand;
    [SerializeField] private HeroController playerHeroController, enemyHeroController;
    [SerializeField] private GameObject[] playerSelectablePanel = new GameObject[6], enemySelectablePanel = new GameObject[6];
    private GameObject[] selectablePanel = new GameObject[12];
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        selectablePanel = playerSelectablePanel.Concat(enemySelectablePanel).ToArray();
    }
    #region ��D�擾
    /// <summary>
    /// player��enemy�̎�D�̃J�[�h���擾����
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsInHand(bool isPlayerHand)
    {
        return (isPlayerHand ? playerHand : enemyHand).OfType<Transform>().Select(i => i.GetComponent<CardController>()).ToList();
    }
    #endregion
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
    /// ����fieldID��GfieldID�� �GfieldID�𖡕�fieldID�ɕϊ�����@�ʐM�ΐ�p
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public int ChangeFieldID(int fieldID)
    {
        if (1 <= fieldID && fieldID <= 6)
        {
            return fieldID + 6;
        }
        else if (7 <= fieldID && fieldID <= 12)
        {
            return fieldID - 6;
        }
        return 0; //��������̂͂܂���
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
            || SkillManager.instance.IsSnipe(attacker.model)) //isSnipe�͂ǂ��ł��U���ł���
        {
            return true;
        }

        if (target.model.isPlayerCard) //target��isPlayerCard�ƁA�U������鑤�̃t�B�[���h�Q���v���C���[�̃t�B�[���h�ł��邩 �͈�v����
        {
            if (SkillManager.instance.IsAnyTaunt(true)) { return false; }

            if (SkillManager.instance.IsBlock(true, target.model.thisFieldID)) { return false; }
        }
        else //����ȊO��field��enemy��
        {
            if (SkillManager.instance.IsAnyTaunt(false)) { return false; }

            if (SkillManager.instance.IsBlock(false, target.model.thisFieldID)) { return false; }

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
        if (SkillManager.instance.IsSnipe(attacker.model)) //isSnipe�͂ǂ��ł��U���ł���
        {
            return true;
        }

        if (target.model.isPlayer) //target��isPlayer�ƁA�U������鑤�̃t�B�[���h�Q���v���C���[�̃t�B�[���h�ł��邩 �͈�v����
        {
            if (SkillManager.instance.IsAnyTaunt(true)) { return false; }

            //�E�H�[��
            if (SkillManager.instance.IsWall(true)) { return false; }
        }
        else //����ȊO��enemy��
        {
            if (SkillManager.instance.IsAnyTaunt(false)) { return false; }

            //�E�H�[��
            if (SkillManager.instance.IsWall(false)) { return false; }
        }
        return true;
    }
    /// <summary>
    /// �I�����ʂɂ����āA�I���\�ȃt�B�[���h�̑I���\�p�l����\������
    /// </summary>
    /// <param name="fieldIDs"></param>
    /// <param name="isActive"></param>
    public void SetSelectablePanel(int[] fieldIDs, bool isActive)
    {
        foreach (var fieldID in fieldIDs)
        {
            selectablePanel[fieldID - 1].SetActive(isActive);
        }
    }
    /// <summary>
    /// �I���\�p�l���̐F��ς���@�����\��̉ӏ��͗ΐF�ŕ\���������̂�
    /// </summary>
    /// <param name="fieldID"></param>
    /// <param name="willSummon"></param>
    public void ChangeSelectablePanelColor(int fieldID, bool willSummon)
    {
        if (willSummon) { selectablePanel[fieldID - 1].GetComponent<Image>().color = new Color(0, 255, 0); }
        else { selectablePanel[fieldID - 1].GetComponent<Image>().color = new Color(255, 0, 0);  }
    }
    #endregion
}
