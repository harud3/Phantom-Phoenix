using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class FieldManager : MonoBehaviour
{
    public static FieldManager instance { get; private set; }
    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private Transform playerHand, enemyHand;
    [SerializeField] private HeroController playerHeroController, enemyHeroController;
    [SerializeField] private GameObject[] playerSelectablePanel = new GameObject[6], enemySelectablePanel = new GameObject[6];
    [SerializeField] private GameObject[] heroSelectablePanel = new GameObject[2];
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
    public List<CardController> GetCardsInHand(bool isPlayerHand)
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
    /// �Y���t�B�[���h��CardController��S�Ď擾����
    /// </summary>
    public List<CardController> GetUnitsByIsPlayer(bool isPlayer)
    {
        if (isPlayer)
        {
            return GetUnitsByFieldID(Enumerable.Range(1, 6).ToArray());
        }
        else
        {
            return GetUnitsByFieldID(Enumerable.Range(7, 6).ToArray());
        }

    }
    /// <summary>
    /// fieldID����Y���t�B�[���h�ɂ���CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�   fieldID��1�`12 1�`6��player 7�`12��enemy
    /// </summary>
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
    public List<CardController> GetUnitsByFieldID(int[] fieldID)
    {
        return fieldID.Select(i => GetUnitByFieldID(i)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldID����Y���t�B�[���h�ɂ���CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�    fieldID��1�`6���w�肷��@isPlayer��true�Ȃ疡�����j�b�g���@false�Ȃ�G���j�b�g��Ԃ� �܂�A���̊֐��̈���fieldID1�`6�́AfieldID7�`12�̐����𕹂�����
    /// </summary>
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
    public List<CardController> GetUnitsByIsPlayerAndFieldID((bool isPlayer, int fieldID)[] iPfID)
    {
        return iPfID.Select(i => GetUnitByIsPlayerAndFieldID(i.isPlayer, i.fieldID)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// �Y���t�B�[���h�̑O��ɂ���CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�
    /// </summary>
    public List<CardController> GetFrontUnitsByIsPlayer(bool isPlayer)
    {
        if (isPlayer)
        {
            return GetUnitsByFieldID(Enumerable.Range(1, 3).ToArray());
        }
        else
        {
            return GetUnitsByFieldID(Enumerable.Range(7, 3).ToArray());
        }
    }
    /// <summary>
    /// �Y���t�B�[���h���ނɂ��郉���_����CardController���擾���� ���j�b�g�����Ȃ��Ȃ�null��Ԃ�
    /// </summary>
    public CardController GetRandomUnits(bool isPlayerField, CardController avoidCC = null)
    {

        if (isPlayerField)
        {
            var x = playerFields.Where(i => i.childCount != 0)?.Select(i => i.GetComponentInChildren<CardController>()).Where(i => avoidCC != null ? i.model.thisFieldID != avoidCC.model.thisFieldID : true).ToList();
            if (!x.Any()) { return null; }
            return x?[UnityEngine.Random.Range(0, x.Count())];
        }
        else
        {
            var x = enemyFields.Where(i => i.childCount != 0)?.Select(i => i.GetComponentInChildren<CardController>()).Where(i => avoidCC != null ? i.model.thisFieldID != avoidCC.model.thisFieldID : true).ToList();
            if (!x.Any()) { return null; }
            return x?[UnityEngine.Random.Range(0, x.Count())];
        }
    }
    /// <summary>
    /// ����fieldID��GfieldID�� �GfieldID�𖡕�fieldID�ɕϊ�����@�ʐM�ΐ�p
    /// </summary>
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
    /// �󂫂̂���t�B�[���h���擾����
    /// </summary>
    public (Transform emptyField, int fieldID) GetEmptyFieldID(bool isPlayer, int avoidFieldID = 99)
    {
        avoidFieldID = avoidFieldID != 99 ? avoidFieldID % 6 : 99;
        if (avoidFieldID != 1 && GetUnitByFieldID(isPlayer ? 1 : 7) == null) { return isPlayer ? (playerFields[0], 1) : (enemyFields[0], 7); }
        else if (avoidFieldID != 2 && GetUnitByFieldID(isPlayer ? 2 : 8) == null) { return isPlayer ? (playerFields[1], 2) : (enemyFields[1], 8); }
        else if (avoidFieldID != 3 && GetUnitByFieldID(isPlayer ? 3 : 9) == null) { return isPlayer ? (playerFields[2], 3) : (enemyFields[2], 9); }
        else if (avoidFieldID != 4 && GetUnitByFieldID(isPlayer ? 4 : 10) == null) { return isPlayer ? (playerFields[3], 4) : (enemyFields[3], 10); }
        else if (avoidFieldID != 5 && GetUnitByFieldID(isPlayer ? 5 : 11) == null) { return isPlayer ? (playerFields[4], 5) : (enemyFields[4], 11); }
        else if (avoidFieldID != 0 && GetUnitByFieldID(isPlayer ? 6 : 12) == null) { return isPlayer ? (playerFields[5], 6) : (enemyFields[5], 12); }
        return (null, 0); //�t�B�[���h���S�Ė��܂��Ă��鎞
    }
    /// <summary>
    /// �󂫂̂���t�B�[���h�Q���擾����
    /// </summary>
    public List<(Transform emptyField, int fieldID)> GetEmptyFieldIDs(bool isPlayer, int avoidFieldID = 99)
    {
        var x = new List<(Transform emptyField, int fieldID)>();
        if (isPlayer)
        {
            foreach (var y in Enumerable.Range(1, 6))
            {
                if (avoidFieldID != y && GetUnitByFieldID(y) == null) { x.Add((playerFields[y - 1], y)); }
            }
        }
        else
        {
            foreach (var y in Enumerable.Range(7, 6))
            {
                if (avoidFieldID != y && GetUnitByFieldID(y) == null) { x.Add((enemyFields[y - 7], y)); }
            }
        }
        return x;
    }
    /// <summary>
    /// �w�肳�ꂽ�t�B�[���h���󂢂Ă���Ȃ�擾����@�����łȂ����(null,0)��Ԃ�
    /// </summary>
    public (Transform emptyField, int fieldID) GetEmptyFieldIDByFieldID(int fieldID)
    {
        if (fieldID <= 6)
        {
            return GetUnitByFieldID(fieldID) == null ? (playerFields[fieldID - 1], fieldID) : (null, 0);
        }
        else
        {
            return GetUnitByFieldID(fieldID) == null ? (enemyFields[fieldID - 7], fieldID) : (null, 0);
        }
    }
    public List<(Transform emptyField, int fieldID)> GetEmptyFieldIDsByFieldID(int[] fieldsID)
    {
        return fieldsID.ToList().Select(fieldID => GetEmptyFieldIDByFieldID(fieldID)).ToList();
    }
    /// <summary>
    /// ���̃t�B�[���h���O�񂩂ǂ���
    /// </summary>
    public bool IsFront(int fieldID)
    {
        switch (fieldID)
        {
            case 1: case 2: case 3: case 7: case 8: case 9:
                return true;
            default: return false; //�܂�1�`12�ȊO�͂Ȃ��낤

        }
    }
    /// <summary>
    /// �󂫂̂���O��̃t�B�[���h���擾����
    /// </summary>
    public (Transform emptyField, int fieldID) GetEmptyFrontFieldID(bool isPlayer)
    {
        if (GetUnitByFieldID(isPlayer ? 1 : 7) == null) { return isPlayer ? (playerFields[0], 1) : (enemyFields[0], 7); }
        else if (GetUnitByFieldID(isPlayer ? 2 : 8) == null) { return isPlayer ? (playerFields[1], 2) : (enemyFields[1], 8); }
        else if (GetUnitByFieldID(isPlayer ? 3 : 9) == null) { return isPlayer ? (playerFields[2], 3) : (enemyFields[2], 9); }
        return (null, 0); //�t�B�[���h���S�Ė��܂��Ă��鎞
    }
    /// <summary>
    /// �󂫂̂�����̃t�B�[���h���擾����
    /// </summary>
    public (Transform emptyField, int fieldID) GetEmptyBackFieldID(bool isPlayer)
    {
        if (GetUnitByFieldID(isPlayer ? 4 : 10) == null) { return isPlayer ? (playerFields[3], 4) : (enemyFields[3], 10); }
        else if (GetUnitByFieldID(isPlayer ? 5 : 11) == null) { return isPlayer ? (playerFields[4], 5) : (enemyFields[4], 11); }
        else if (GetUnitByFieldID(isPlayer ? 6 : 12) == null) { return isPlayer ? (playerFields[5], 6) : (enemyFields[5], 12); }
        return (null, 0); //�t�B�[���h���S�Ė��܂��Ă��鎞
    }
    /// <summary>
    /// �󂫂̂���㉺�̃t�B�[���h�Q���擾����
    /// </summary>
    public List<(Transform emptyField, int fieldID)> GetEmptyUpDownFieldID(int fieldID)
    {
        if (fieldID < 1 || 12 < fieldID) { return null; } //�z��O�̒l�΍�
        else if (fieldID % 3 == 1) { return GetEmptyFieldIDsByFieldID(new int[] { fieldID + 1 }); } //fieldID����i
        else if (fieldID % 3 == 2) { return GetEmptyFieldIDsByFieldID(new int[] { fieldID - 1, fieldID + 1 }); } //fieldID�����i
        else { return GetEmptyFieldIDsByFieldID(new int[] { fieldID - 1 }); } //fieldID�����i
    }
    /// <summary>
    /// �Ώۃ��j�b�g���U���\���ǂ����𔻒肷��
    /// </summary>
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
    #endregion
    #region �Ֆʏ���
    /// <summary>
    /// �Ώۃq�[���[���U���\���ǂ������肷��
    /// </summary>
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
    /// �I�����ʂɂ����āA�I���\�ȃq�[���[�̑I���\�p�l����\������ 1�Ԃ��w�肷���playerHero 2�Ԃ��w�肷���enemyHero
    /// </summary>
    /// <param name="fieldIDs"></param>
    /// <param name="isActive"></param>
    public void SetHeroSelectablePanel(int[] heros, bool isActive)
    {
        foreach (var hero in heros)
        {
            heroSelectablePanel[hero - 1].SetActive(isActive);
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
        else { selectablePanel[fieldID - 1].GetComponent<Image>().color = new Color(255, 0, 0); }
    }
    #endregion
    #region �t�B�[���h���j�b�g��
    /// <summary>
    /// �e�t�B�[���h�̃��j�b�g����ݒ肷��
    /// </summary>
    public void SetFieldOnUnitcnt(bool isPlayerField)
    {
        if (isPlayerField)
        {
            playerFieldOnUnitCnt = GetUnitsByFieldID(new int[] { 1, 2, 3, 4, 5, 6 })?.Count ?? 0;
        }
        else
        {
            enemyFieldOnUnitCnt = GetUnitsByFieldID(new int[] { 7, 8, 9, 10, 11, 12 })?.Count ?? 0;
        }
    }
    /// <summary>
    /// �t�B�[���h�ɂ��郆�j�b�g�̐������炷
    /// </summary>
    /// <param name="isPlayerField"></param>
    public void Minus1FieldOnUnitCnt(bool isPlayerField)
    {
        if (isPlayerField)
        {
            if (playerFieldOnUnitCnt > 0)
            {
                playerFieldOnUnitCnt -= 1;
            }
        }
        else
        {
            if (enemyFieldOnUnitCnt > 0)
            {
                enemyFieldOnUnitCnt -= 1;
            }
        }
    }
    private int _playerFieldOnUnitCnt;
    public int playerFieldOnUnitCnt
    {
        get { return _playerFieldOnUnitCnt; }
        private set
        {
            _playerFieldOnUnitCnt = value;
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in playerHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
            if (GameManager.instance.isPlayerTurn)
            {
                GameManager.instance.SetCanSummonHandCards();
            }

        }
    }
    private int _enemyFieldOnUnitCnt;
    public int enemyFieldOnUnitCnt
    {
        get { return _enemyFieldOnUnitCnt; }
        private set
        {
            _enemyFieldOnUnitCnt = value;
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in enemyHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
            if (GameManager.instance.isPlayerTurn)
            {
                GameManager.instance.SetCanSummonHandCards();
            }
        }
    }
    #endregion
    #region ��n
    private List<(int cardID, int cost)> _playerCatacombe = new List<(int cardID, int cost)>();
    private List<(int cardID, int cost)> _enemyCatacombe = new List<(int cardID, int cost)>();
    public List<(int cardID, int cost)> playerCatacombe { get { return _playerCatacombe; } private set { _playerCatacombe = value; } }
    public List<(int cardID, int cost)> enemyCatacombe { get { return _enemyCatacombe; } private set { _enemyCatacombe = value; } }
    public void AddCatacombe(CardModel diedUnit)
    {
        if (diedUnit.isPlayerCard)
        {
            playerCatacombe.Add((diedUnit.cardID, diedUnit.defaultCost));
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in playerHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
            if (GameManager.instance.isPlayerTurn)
            {
                GameManager.instance.SetCanSummonHandCards();
            }
        }
        else
        {
            enemyCatacombe.Add((diedUnit.cardID, diedUnit.defaultCost));
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in enemyHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
        }
    }
    #endregion
    #region �X�y���g�p����
    private List<(int cardID, int cost)> _playerUsedSpell = new List<(int cardID, int cost)>();
    private List<(int cardID, int cost)> _enemyUsedSpell = new List<(int cardID, int cost)>();
    public List<(int cardID, int cost)> playerUsedSpellList { get { return _playerUsedSpell; } private set { _playerUsedSpell = value; } }
    public List<(int cardID, int cost)> enemyUsedSpellList { get { return _enemyUsedSpell; } private set { _enemyUsedSpell = value; } }
    public void AddSpellList(CardModel usedSpell)
    {
        if (usedSpell.isPlayerCard)
        {
            playerUsedSpellList.Add((usedSpell.cardID, usedSpell.defaultCost));
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in playerHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
            if (GameManager.instance.isPlayerTurn)
            {
                GameManager.instance.SetCanSummonHandCards();
            }
        }
        else
        {
            enemyUsedSpellList.Add((usedSpell.cardID, usedSpell.defaultCost));

        }
    }
    #endregion
    #region �A�C�e���J�[�h�ɂ�鋭���񐔁@king�p
    private int _playerBuffedCntByItemCard = 0;
    private int _enemyBuffedCntByItemCard = 0;
    public int playerBuffedCntByItemCard { get { return _playerBuffedCntByItemCard; } private set { _playerBuffedCntByItemCard = value; } }
    public int enemyBuffedCntByItemCard { get { return _enemyBuffedCntByItemCard; } private set { _enemyBuffedCntByItemCard = value; } }
    public void AddBuffCntByItemCard(bool isPlayer)
    {
        if (isPlayer)
        {
            playerBuffedCntByItemCard++;
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in playerHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
            if (GameManager.instance.isPlayerTurn)
            {
                GameManager.instance.SetCanSummonHandCards();
            }
        }
        else
        {
            enemyBuffedCntByItemCard++;
            GetUnitsByFieldID(Enumerable.Range(1, 12).ToArray())?.ForEach(i => i.UpdateSkill?.Invoke());
            foreach (Transform item in enemyHand)
            {
                item.GetComponent<CardController>().UpdateSkill?.Invoke();
            }
            if (GameManager.instance.isPlayerTurn)
            {
                GameManager.instance.SetCanSummonHandCards();
            }
        }
    }
    #endregion
}
