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
    /// fieldIDから該当フィールドにあるCardControllerを取得する ユニットがいないならnullを返す   fieldIDは1～12 1～6がplayer 7～12がenemy
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
    /// fieldID[]から該当フィールドにあるCardController[]を取得する ユニットがいないならnullを返すと思う...     fieldIDは1～12 1～6がplayer 7～12がenemy
    /// </summary>
    /// <param name="fieldID"></param>
    /// <returns></returns>
    public List<CardController> GetUnitsByFieldID(int[] fieldID)
    {
        return fieldID.Select(i => GetUnitByFieldID(i)).Where(i => i != null).ToList();
    }
    /// <summary>
    /// fieldIDから該当フィールドにあるCardControllerを取得する ユニットがいないならnullを返す    fieldIDは1～6を指定する　isPlayerがtrueなら味方ユニットを　falseなら敵ユニットを返す つまり、この関数の引数fieldID1～6は、fieldID7～12の性質を併せ持つ
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
    /// fieldID[]から該当フィールドにあるCardController[]を取得する ユニットがいないならnullを返すと思う...   fieldIDは1～6を指定する　isPlayerがtrueなら味方ユニットを　falseなら敵ユニットを返す つまり、この関数の引数fieldID1～6は、fieldID7～12の性質を併せ持つ
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
