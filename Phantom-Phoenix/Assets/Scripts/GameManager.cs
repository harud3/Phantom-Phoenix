using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Data;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    [SerializeField] private HeroController playerHeroController, enemyHeroController;

    [SerializeField] private Transform[] playerFields = new Transform[6], enemyFields = new Transform[6];
    [SerializeField] private Transform playerHandTransform, enemyHandTransform;
    
    [SerializeField] private CardController cardPrefab;

    [SerializeField] private UnityEngine.UI.Button turnButton;
    [SerializeField] private Sprite playerTurnSprite,enemyTurnSprite; //ターン終了、相手のターンのスプライト

    [SerializeField] private Image TimeLimit1, TimeLimit2, TimeLimit3; //時間制限の表示部
    [SerializeField] int timeLimit; //ターンの時間制限
    int timeCount; //ターンの残り時間

    private bool _isPlayerTurn;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image resultImage;

    //TODO:あとあと外部から決められるように
    int playerHeroID  = 1, enemyHeroID = 1;
    List<int> playerDeck = new List<int>(){1, 1, 6,7, 8,5,2,5,6,7,8,8,7, 2, 2,4,4,5,5,5};
    List<int> enemyDeck = new List<int>(){ 1,1,1,1,1,1,1, 2,3,3,4,5};
    #region 初期設定
    void Start()
    {
        StartGame();
    }
    void StartGame()
    {
        SetTime();
        SettingInitHero();
        SettingInitHand();
        isPlayerTurn = UnityEngine.Random.Range(0, 2) == 0;
        TurnCalc();
    }
    //ヒーローの設定
    void SettingInitHero()
    {
        playerHeroController.Init(playerHeroID);
        enemyHeroController.Init(enemyHeroID);
    }
    /// <summary>
    /// 初期手札の設定
    /// </summary>
    void SettingInitHand()
    {
        for (int i = 0; i < 3; i++) //初期手札は3枚
        {
            GiveCardToHand(playerDeck, playerHandTransform, playerHeroController.model.isPlayer);
            GiveCardToHand(enemyDeck, enemyHandTransform, enemyHeroController.model.isPlayer);
        }
    }
    #endregion
    #region　時間管理
    void SetTime()
    {
        int tl1 = timeCount / 100;
        TimeLimit1.sprite = Resources.Load<Sprite>($"Numbers/s{tl1}");
        if (tl1 == 0) { TimeLimit1.enabled = false; } else { TimeLimit1.enabled = true; }
        int tl2 = timeCount / 10;
        TimeLimit2.sprite = Resources.Load<Sprite>($"Numbers/s{tl2}");
        if (tl2 == 0) { TimeLimit2.enabled = false; } else { TimeLimit2.enabled = true; }
        TimeLimit3.sprite = Resources.Load<Sprite>($"Numbers/s{timeCount % 10}");
    }
    IEnumerator CountDown()
    {
        timeCount = timeLimit;
        SetTime();
        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1);
            timeCount--;
            SetTime();
        }
        ChangeTurn();
    }
    #endregion
    #region カード生成
    /// <summary>
    /// デッキから手札にカードを配る
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    void GiveCardToHand(List<int> deck,Transform hand, bool isPlayer)
    {
        if (deck.Count == 0) //TODO:デッキがないなら戻る　ドローごとに1ダメージ→2ダメージ→3ダメージ...にする
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID, hand, isPlayer);
    }
    /// <summary>
    /// カードの生成
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="hand"></param>
    /// <param name="isPlayer"></param>
    void CreateCard(int cardID, Transform hand, bool isPlayer)
    {
        if (hand.childCount == 10) { return; }
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(cardID, isPlayer);
    }
    #endregion

    #region　ターン制御
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            GiveCardToHand(playerDeck, playerHandTransform, playerHeroController.model.isPlayer);
        }
        else
        {
            GiveCardToHand(enemyDeck, enemyHandTransform, enemyHeroController.model.isPlayer);
        }
        TurnCalc();
    }
    void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
        if (isPlayerTurn) {
            PlayerTurn();
        }
        else {
            StartCoroutine(EnemyTurn());
        }
    }
    #endregion
    #region より具体的なターン制御
    void SetCanAttackAllFieldUnit(Transform[] fields, bool CanAttack)
    {
        foreach (var field in fields)
        {
            if (field.childCount != 0)
            {
                field.GetChild(0).GetComponent<CardController>().SetCanAttack(CanAttack);
            }
        }
    }
    void PlayerTurn()
    {
        turnButton.image.sprite = playerTurnSprite;
        Debug.Log("味方ターン");
        playerHeroController.ResetMP();
        SetCanAttackAllFieldUnit(playerFields, true);
        SetCanAttackAllFieldUnit(enemyFields, false);
    }
    IEnumerator EnemyTurn()
    {
        turnButton.image.sprite = enemyTurnSprite;
        Debug.Log("相手ターン");
        enemyHeroController.ResetMP();
        SetCanAttackAllFieldUnit(playerFields, false);
        SetCanAttackAllFieldUnit(enemyFields, true);

        //enemyAIが動かす
        //手札から出せるカードを出す
        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        if (handCardList.Length != 0)
        {
            foreach (var canPutCard in handCardList.Where(i => i.model.cost <= GetHeroMP(false)))
            {
                if(canPutCard.model.cost > GetHeroMP(false)) { continue; }
                foreach (var enemyField in enemyFields)
                {
                    if (enemyField.childCount == 0)
                    {
                        StartCoroutine(canPutCard.movement.MoveToField(enemyField));
                        yield return new WaitForSeconds(0.25f);
                        canPutCard.MoveField(enemyField.GetComponent<DropField>().fieldID);
                        canPutCard.putOnField(false);
                        yield return new WaitForSeconds(0.75f);
                        break;
                    }
                }
            }

        }

        //fieldの攻撃可能ユニットで攻撃する 味方ユニットがいるならユニットを　味方ユニットがいないならヒーローを攻撃
        IEnumerable<CardController> canAttackFieldEnemyCards 
            = enemyFields.Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.canAttack);
        foreach (var canAttackFieldEnemyCard in canAttackFieldEnemyCards)
        {
            foreach (var playerField in playerFields)
            {
                if (playerField.childCount != 0)
                {
                    if( playerField.GetComponentInChildren<CardController>() is var cc && cc.model.isAlive)
                    {
                        if (!cc.model.is挑発)
                        {
                            if (isAny挑発(true)) { continue; }
                            if (isBlock(true, playerField.GetComponent<DropField>().fieldID)) { continue; }
                        }

                        StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerField));
                        yield return new WaitForSeconds(1f);
                        CardsBattle(canAttackFieldEnemyCard,cc);
                        yield return null;
                        break;
                    }
                    
                }
            }
            if (canAttackFieldEnemyCard.model.canAttack && playerHeroController.model.isAlive)
            {
                if (isAny挑発(true)) { continue; }
                if (isWall(true)) { continue; }

                StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerHeroController.transform));
                yield return new WaitForSeconds(1f);
                AttackTohero(canAttackFieldEnemyCard, false);
                yield return null;
            }
        }

        yield return new WaitForSeconds(1f);
        
        ChangeTurn();
    }
    #endregion
    #region 行動
    public void CardsBattle(CardController attacker, CardController target)
    {

        Debug.Log($"{(isPlayerTurn? "味方ターン" :"相手ターン")} {attacker.model.name}から{target.model.name}に攻撃");
        
        attacker.Attack(target);
        target.Attack(attacker);

        Debug.Log($"{target.model.name}に{attacker.model.atk}ダメージ {target.model.name}の残りHP{target.model.hp}");
        Debug.Log($"{attacker.model.name}に{target.model.atk}ダメージ {attacker.model.name}の残りHP{attacker.model.hp}");

        attacker.CheckAlive();
        target.CheckAlive();
    }
    public void AttackTohero(CardController attacker, bool isPlayerCard)
    {
        if (isPlayerCard) { 
            enemyHeroController.Damage(attacker);
            //勝利判定も
            if (!enemyHeroController.model.isAlive) { 
                Invoke("ViewResultPanel",1f);
            }
        }
        else {
            playerHeroController.Damage(attacker);
            //勝利判定も
            if (!playerHeroController.model.isAlive) {
                Invoke("ViewResultPanel", 1f);
            }
        }
    }
    #endregion
    #region MP操作
    public int GetHeroMP(bool isPlayer)
    {
        return isPlayer ? playerHeroController.model.mp : enemyHeroController.model.mp;
    }
    public void ReduceMP(int reduce, bool isPlayerCard) {
        if (isPlayerCard)
        {
            playerHeroController.ReduceMP(reduce);
        }
        else
        {
            enemyHeroController.ReduceMP(reduce);
        }
    }

    #endregion
    #region 盤面取得
    /// <summary>
    /// FieldIDは1～12 1～6がplayer 7～12がenemy
    /// </summary>
    /// <param name="FieldID"></param>
    /// <returns></returns>
    public CardController GetCardbyFieldID(int FieldID)
    {
        if (1 <= FieldID && FieldID <= 6)
        {
            if (playerFields[FieldID - 1].childCount != 0)
            {
                return playerFields[FieldID-1].GetComponentInChildren<CardController>(); ;
            }
        }
        else if(FieldID <= 12)
        {
            if (enemyFields[FieldID - 7].childCount != 0)
            {
                return enemyFields[FieldID - 7].GetComponentInChildren<CardController>(); ;
            }
        }
        return null;
    }
    public bool isAny挑発(bool isPlayerField)
    {
        
        if (isPlayerField)
        {
            //playerFieldsのfieldID1,2,3のうち、カードが紐づいているfieldを抽出し、そのカード群の中に is挑発 == true なものがあるなら、trueを返す
            if (playerFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.is挑発).Count() > 0) { return true; }
        }
        else
        {
            //enemyFieldsのfieldID1,2,3のうち、カードが紐づいているfieldを抽出し、そのカード群の中に is挑発 == true なものがあるなら、trueを返す
            if (enemyFields.Take(3).Where(i => i.childCount != 0).Select(i => i.GetComponentInChildren<CardController>()).Where(i => i.model.is挑発).Count() > 0) { return true; }
        }
        return false;
    }
    public bool isBlock(bool isPlayerField, int thisFieldID) {
        //fieldIDは、　
        //             後列前列    前列後列
        //              4   1   |   7   10
        //playerHero    5   2   |   8   11  enemyHero
        //              6   3   |   9   12
        //となっている
        if (isPlayerField) {
            //ブロック
            //cardがnull以外なら、すぐ前にユニットがいるためブロックが成立している
            if ((thisFieldID == 4 && GameManager.instance.GetCardbyFieldID(1) != null)
                || (thisFieldID == 5 && GameManager.instance.GetCardbyFieldID(2) != null)
                || (thisFieldID == 6 && GameManager.instance.GetCardbyFieldID(3) != null)
                ) { return true; }
        }
        else
        {
            //ブロック
            if ((thisFieldID == 10 && GameManager.instance.GetCardbyFieldID(7) != null)
                || (thisFieldID == 11 && GameManager.instance.GetCardbyFieldID(8) != null)
                || (thisFieldID == 12 && GameManager.instance.GetCardbyFieldID(9) != null)
                ) { return true; }
        }
        
        return false;
    }
    public bool isWall(bool isPlayerField) {
        if (isPlayerField
            && (GetCardbyFieldID(1) != null || GetCardbyFieldID(4) != null)
            && (GetCardbyFieldID(2) != null || GetCardbyFieldID(5) != null)
            && (GetCardbyFieldID(3) != null || GetCardbyFieldID(6) != null)
            )
        {
            return true;
        }
        else if(!isPlayerField
            && (GetCardbyFieldID(7) != null || GetCardbyFieldID(10) != null)
            && (GetCardbyFieldID(8) != null || GetCardbyFieldID(11) != null)
            && (GetCardbyFieldID(9) != null || GetCardbyFieldID(12) != null)
            )
        {
            return true;
        }
        return false; 
    }
    #endregion
    #region resultとメニューへの遷移
    private void ViewResultPanel()
    {
        resultPanel.SetActive(true);
        if (playerHeroController.model.isAlive && !enemyHeroController.model.isAlive) { resultImage.sprite = Resources.Load<Sprite>($"UIs/win"); }
        else if (!playerHeroController.model.isAlive && enemyHeroController.model.isAlive) { resultImage.sprite = Resources.Load<Sprite>($"UIs/lose"); }
        else if (!playerHeroController.model.isAlive && !enemyHeroController.model.isAlive) { resultImage.sprite = Resources.Load<Sprite>($"UIs/draw"); }
        StopAllCoroutines();
        Invoke("ChangeMenuScene", 5);
    }
    private void ChangeMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
    }
    #endregion
}
