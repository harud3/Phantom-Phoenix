using Photon.Realtime;
using System;
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
    [SerializeField] private GameObject[] heroSelectablePanel = new GameObject[2];
    private GameObject[] selectablePanel = new GameObject[12];
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
            GameManager.instance.SetCanSummonHandCards();
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
            GameManager.instance.SetCanSummonHandCards();
        }
    }
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
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsInHand(bool isPlayerHand)
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
    /// fieldIDから該当フィールドにあるCardControllerを取得する ユニットがいないならnullを返す   fieldIDは1〜12 1〜6がplayer 7〜12がenemy
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
    /// fieldID[]から該当フィールドにあるCardController[]を取得する ユニットがいないならnullを返すと思う...     fieldIDは1〜12 1〜6がplayer 7〜12がenemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsByFieldID(int[] fieldID)
    {
        return fieldID.Select(i => GetUnitByFieldID(i)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldIDから該当フィールドにあるCardControllerを取得する ユニットがいないならnullを返す    fieldIDは1〜6を指定する　isPlayerがtrueなら味方ユニットを　falseなら敵ユニットを返す つまり、この関数の引数fieldID1〜6は、fieldID7〜12の性質を併せ持つ
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
    /// fieldID[]から該当フィールドにあるCardController[]を取得する ユニットがいないならnullを返すと思う...   fieldIDは1〜6を指定する　isPlayerがtrueなら味方ユニットを　falseなら敵ユニットを返す つまり、この関数の引数fieldID1〜6は、fieldID7〜12の性質を併せ持つ
    /// </summary>
    /// <param name="iPfID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsByIsPlayerAndFieldID((bool isPlayer, int fieldID)[] iPfID)
    {
        return iPfID.Select(i => GetUnitByIsPlayerAndFieldID(i.isPlayer, i.fieldID)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// 該当フィールド分類にいるランダムなCardControllerを取得する ユニットがいないならnullを返す
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public CardController GetRandomUnits(bool isPlayerField)
    {

        if (isPlayerField)
        {
            var x = playerFields.Where(i => i.childCount != 0)?.Select(i => i.GetComponentInChildren<CardController>()).ToList();
            if (!x.Any()) { return null; }
            return x?[UnityEngine.Random.Range(0, x.Count())];
        }
        else
        {
            var x = enemyFields.Where(i => i.childCount != 0)?.Select(i => i.GetComponentInChildren<CardController>()).ToList();
            if (!x.Any()) { return null; }
            return x?[UnityEngine.Random.Range(0, x.Count())];
        }
    }
    /// <summary>
    /// 味方fieldIDを敵fieldIDに 敵fieldIDを味方fieldIDに変換する　通信対戦用
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
        return 0; //ここ来るのはまずい
    }
    /// <summary>
    /// 空きのあるフィールドを取得する
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public (Transform emptyField, int fieldID) GetEmptyFieldID(bool isPlayer)
    {
        if (GetUnitByFieldID(isPlayer ? 1 : 7) == null) { return isPlayer ? (playerFields[0],1) : (enemyFields[0], 7); }
        else if (GetUnitByFieldID(isPlayer ? 2 : 8) == null) { return isPlayer ? (playerFields[1], 2) : (enemyFields[1], 8); }
        else if (GetUnitByFieldID(isPlayer ? 3 : 9) == null) { return isPlayer ? (playerFields[2], 3) : (enemyFields[2], 9); }
        else if (GetUnitByFieldID(isPlayer ? 4 : 10) == null) { return isPlayer ? (playerFields[3], 4) : (enemyFields[3], 10); }
        else if (GetUnitByFieldID(isPlayer ? 5 : 11) == null) { return isPlayer ? (playerFields[4], 5) : (enemyFields[4], 11); }
        else if (GetUnitByFieldID(isPlayer ? 6 : 12) == null) { return isPlayer ? (playerFields[5], 6) : (enemyFields[5], 12); }
        return (null,0); //フィールドが全て埋まっている時
    }
    public bool IsFront(int fieldID)
    {
        switch (fieldID)
        {
            case 1: case 2: case 3: case 7: case 8: case 9:
                return true;
            default: return false; //まあ1〜12以外はなかろう

        }
    }
    /// <summary>
    /// 空きのある前列のフィールドを取得する
    /// </summary>
    /// <param name="isPlayer"></param>
    /// <returns></returns>
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
    /// <param name="isPlayer"></param>
    /// <returns></returns>
    public (Transform emptyField, int fieldID) GetEmptyBackFieldID(bool isPlayer)
    {
        if (GetUnitByFieldID(isPlayer ? 4 : 10) == null) { return isPlayer ? (playerFields[3], 4) : (enemyFields[3], 10); }
        else if (GetUnitByFieldID(isPlayer ? 5 : 11) == null) { return isPlayer ? (playerFields[4], 5) : (enemyFields[4], 11); }
        else if (GetUnitByFieldID(isPlayer ? 6 : 12) == null) { return isPlayer ? (playerFields[5], 6) : (enemyFields[5], 12); }
        return (null, 0); //フィールドが全て埋まっている時
    }
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
    public void Minus1FieldOnUnitCnt(bool isPlayerField)
    {
        if (isPlayerField)
        {
            playerFieldOnUnitCnt -= 1;
        }
        else
        {
            enemyFieldOnUnitCnt -= 1;
        }
    }
    /// <summary>
    /// 対象ユニットを攻撃可能かどうかを判定する
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
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
    /// <summary>
    /// 対象ヒーローを攻撃可能かどうか判定する
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    /// <returns></returns>
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
        else { selectablePanel[fieldID - 1].GetComponent<Image>().color = new Color(255, 0, 0);  }
    }
    #endregion
}
