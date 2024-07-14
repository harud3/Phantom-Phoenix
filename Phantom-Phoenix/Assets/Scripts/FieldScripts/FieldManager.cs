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
    #region 手札取得
    /// <summary>
    /// playerかenemyの手札のカードを取得する
    /// </summary>
    public List<CardController> GetCardsInHand(bool isPlayerHand)
    {
        return (isPlayerHand ? playerHand : enemyHand).OfType<Transform>().Select(i => i.GetComponent<CardController>()).ToList();
    }
    #endregion
    #region 盤面取得
    //fieldIDは、　
    //             後列前列    前列後列
    //              4   1   |   7   10
    //playerHero    5   2   |   8   11  enemyHero
    //              6   3   |   9   12
    //となっている
    /// <summary>
    /// 該当フィールドのCardControllerを全て取得する
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
    /// fieldIDから該当フィールドにあるCardControllerを取得する ユニットがいないならnullを返す   fieldIDは1～12 1～6がplayer 7～12がenemy
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
    /// fieldID[]から該当フィールドにあるCardController[]を取得する ユニットがいないならnullを返すと思う...     fieldIDは1～12 1～6がplayer 7～12がenemy
    /// </summary>
    public List<CardController> GetUnitsByFieldID(int[] fieldID)
    {
        return fieldID.Select(i => GetUnitByFieldID(i)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldIDから該当フィールドにあるCardControllerを取得する ユニットがいないならnullを返す    fieldIDは1～6を指定する　isPlayerがtrueなら味方ユニットを　falseなら敵ユニットを返す つまり、この関数の引数fieldID1～6は、fieldID7～12の性質を併せ持つ
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
    /// fieldID[]から該当フィールドにあるCardController[]を取得する ユニットがいないならnullを返すと思う...   fieldIDは1～6を指定する　isPlayerがtrueなら味方ユニットを　falseなら敵ユニットを返す つまり、この関数の引数fieldID1～6は、fieldID7～12の性質を併せ持つ
    /// </summary>
    public List<CardController> GetUnitsByIsPlayerAndFieldID((bool isPlayer, int fieldID)[] iPfID)
    {
        return iPfID.Select(i => GetUnitByIsPlayerAndFieldID(i.isPlayer, i.fieldID)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// 該当フィールドの前列にいるCardControllerを取得する ユニットがいないならnullを返す
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
    /// 該当フィールド分類にいるランダムなCardControllerを取得する ユニットがいないならnullを返す
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
    /// 味方fieldIDを敵fieldIDに 敵fieldIDを味方fieldIDに変換する　通信対戦用
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
        return 0; //ここ来るのはまずい
    }
    /// <summary>
    /// 空きのあるフィールドを取得する
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
        return (null, 0); //フィールドが全て埋まっている時
    }
    /// <summary>
    /// 空きのあるフィールド群を取得する
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
    /// 指定されたフィールドが空いているなら取得する　そうでなければ(null,0)を返す
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
    /// そのフィールドが前列かどうか
    /// </summary>
    public bool IsFront(int fieldID)
    {
        switch (fieldID)
        {
            case 1: case 2: case 3: case 7: case 8: case 9:
                return true;
            default: return false; //まあ1～12以外はなかろう

        }
    }
    /// <summary>
    /// 空きのある前列のフィールドを取得する
    /// </summary>
    public (Transform emptyField, int fieldID) GetEmptyFrontFieldID(bool isPlayer)
    {
        if (GetUnitByFieldID(isPlayer ? 1 : 7) == null) { return isPlayer ? (playerFields[0], 1) : (enemyFields[0], 7); }
        else if (GetUnitByFieldID(isPlayer ? 2 : 8) == null) { return isPlayer ? (playerFields[1], 2) : (enemyFields[1], 8); }
        else if (GetUnitByFieldID(isPlayer ? 3 : 9) == null) { return isPlayer ? (playerFields[2], 3) : (enemyFields[2], 9); }
        return (null, 0); //フィールドが全て埋まっている時
    }
    /// <summary>
    /// 空きのある後列のフィールドを取得する
    /// </summary>
    public (Transform emptyField, int fieldID) GetEmptyBackFieldID(bool isPlayer)
    {
        if (GetUnitByFieldID(isPlayer ? 4 : 10) == null) { return isPlayer ? (playerFields[3], 4) : (enemyFields[3], 10); }
        else if (GetUnitByFieldID(isPlayer ? 5 : 11) == null) { return isPlayer ? (playerFields[4], 5) : (enemyFields[4], 11); }
        else if (GetUnitByFieldID(isPlayer ? 6 : 12) == null) { return isPlayer ? (playerFields[5], 6) : (enemyFields[5], 12); }
        return (null, 0); //フィールドが全て埋まっている時
    }
    /// <summary>
    /// 空きのある上下のフィールド群を取得する
    /// </summary>
    public List<(Transform emptyField, int fieldID)> GetEmptyUpDownFieldID(int fieldID)
    {
        if (fieldID < 1 || 12 < fieldID) { return null; } //想定外の値対策
        else if (fieldID % 3 == 1) { return GetEmptyFieldIDsByFieldID(new int[] { fieldID + 1 }); } //fieldIDが上段
        else if (fieldID % 3 == 2) { return GetEmptyFieldIDsByFieldID(new int[] { fieldID - 1, fieldID + 1 }); } //fieldIDが中段
        else { return GetEmptyFieldIDsByFieldID(new int[] { fieldID - 1 }); } //fieldIDが下段
    }
    /// <summary>
    /// 対象ユニットを攻撃可能かどうかを判定する
    /// </summary>
    public bool CheckCanAttackUnit(CardController attacker, CardController target)
    {
        //ブロックや挑発がされているならをゴリ押し構文で判定する スマートな方法を思いつけなかった…
        //このスクリプトがアタッチされているカードの親(つまり置かれているfield)のfieldIDを取得する
        //fieldIDは、　
        //             後列前列    前列後列
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //となっている
        if (target.model.isTaunt   /*isTauntはfield1,2,3またはfield7,8,9にいる時にtrueとなる設定　よって、targetがisTauntしてるなら即開戦でOK */
            || SkillManager.instance.IsSnipe(attacker.model)) //isSnipeはどこでも攻撃できる
        {
            return true;
        }

        if (target.model.isPlayerCard) //targetのisPlayerCardと、攻撃される側のフィールド群がプレイヤーのフィールドであるか は一致する
        {
            if (SkillManager.instance.IsAnyTaunt(true)) { return false; }

            if (SkillManager.instance.IsBlock(true, target.model.thisFieldID)) { return false; }
        }
        else //それ以外のfieldはenemy側
        {
            if (SkillManager.instance.IsAnyTaunt(false)) { return false; }

            if (SkillManager.instance.IsBlock(false, target.model.thisFieldID)) { return false; }

        }
        return true;
    }
    #endregion
    #region 盤面処理
    /// <summary>
    /// 対象ヒーローを攻撃可能かどうか判定する
    /// </summary>
    public bool CheckCanAttackHero(CardController attacker, HeroController target)
    {
        //ブロックや挑発がされているならをゴリ押し構文で判定する スマートな方法を思いつけなかった…
        //このスクリプトがアタッチされているカードの親(つまり置かれているfield)のfieldIDを取得する
        //fieldIDは、　
        //             後列前列    前列後列
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //となっている
        if (SkillManager.instance.IsSnipe(attacker.model)) //isSnipeはどこでも攻撃できる
        {
            return true;
        }

        if (target.model.isPlayer) //targetのisPlayerと、攻撃される側のフィールド群がプレイヤーのフィールドであるか は一致する
        {
            if (SkillManager.instance.IsAnyTaunt(true)) { return false; }

            //ウォール
            if (SkillManager.instance.IsWall(true)) { return false; }
        }
        else //それ以外はenemy側
        {
            if (SkillManager.instance.IsAnyTaunt(false)) { return false; }

            //ウォール
            if (SkillManager.instance.IsWall(false)) { return false; }
        }
        return true;
    }
    /// <summary>
    /// 選択効果において、選択可能なフィールドの選択可能パネルを表示する
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
    /// 選択効果において、選択可能なヒーローの選択可能パネルを表示する 1番を指定するとplayerHero 2番を指定するとenemyHero
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
    /// 選択可能パネルの色を変える　召喚予定の箇所は緑色で表示したいので
    /// </summary>
    /// <param name="fieldID"></param>
    /// <param name="willSummon"></param>
    public void ChangeSelectablePanelColor(int fieldID, bool willSummon)
    {
        if (willSummon) { selectablePanel[fieldID - 1].GetComponent<Image>().color = new Color(0, 255, 0); }
        else { selectablePanel[fieldID - 1].GetComponent<Image>().color = new Color(255, 0, 0); }
    }
    #endregion
    #region フィールドユニット数
    /// <summary>
    /// 各フィールドのユニット数を設定する
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
    /// フィールドにいるユニットの数を減らす
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
    #region 墓地
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
    #region スペル使用履歴
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
    #region アイテムカードによる強化回数　king用
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
