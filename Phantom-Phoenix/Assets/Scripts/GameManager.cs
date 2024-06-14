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

public class GameManager : MonoBehaviour
{
    [SerializeField]
    Transform 
        playerHandTransform,
        playerFieldTranform1, playerFieldTranform2, playerFieldTranform3, playerFieldTranform4, playerFieldTranform5, playerFieldTranform6,
        enemyHandTransform,
        enemyFieldTranform7, enemyFieldTranform8, enemyFieldTranform9, enemyFieldTranform10, enemyFieldTranform11, enemyFieldTranform12;
    [SerializeField] CardController cardPrefab;
    [SerializeField] Button button;
    [SerializeField] Sprite playerTurnSprite; 
    [SerializeField] Sprite enemyTurnSprite;
    bool _isPlayerTurn;

    [SerializeField] Image TimeLimit1, TimeLimit2, TimeLimit3;
    [SerializeField] int timeLimit;
    int timeCount;

    private Transform[] playerFields, enemyFields;
    [NonSerialized]
    public static GameManager instance;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }

    [SerializeField] GameObject resultPanel;
    [SerializeField] Image resultImage;

    int playerHeroID  = 1, enemyHeroID = 1;
    List<int> playerDeck = new List<int>(){1, 1, 6,7, 8,5,2,5,6,7,8,8,7, 2, 2,4,4,5,5,5};
    List<int> enemyDeck = new List<int>(){ 2, 1, 6,6,6,6,7,8,2, 2,3,3,4,5};
    [SerializeField] HeroController playerHeroController,enemyHeroController;
    void SettingInitHero()
    {
        playerHeroController.Init(playerHeroID);
        enemyHeroController.Init(enemyHeroID);
    }

    void SettingInitHand()
    {
        for (int i = 0; i < 3; i++)
        {
            GiveCardToHand(playerDeck, playerHandTransform);
            GiveCardToHand(enemyDeck, enemyHandTransform);
        }
    }
    void GiveCardToHand(List<int> deck,Transform hand)
    {
        if (deck.Count == 0)
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID, hand);
    }
    void CreateCard(int cardID, Transform hand)
    {
        if (hand.childCount == 10) { return; }
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(cardID, hand.tag == "PlayerHand" ? true : false);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        playerFields = new Transform[] { playerFieldTranform1, playerFieldTranform2, playerFieldTranform3, playerFieldTranform4, playerFieldTranform5, playerFieldTranform6 };
        enemyFields = new Transform[] { enemyFieldTranform7, enemyFieldTranform8, enemyFieldTranform9, enemyFieldTranform10, enemyFieldTranform11, enemyFieldTranform12 };
        StartGame();
    }
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
    void StartGame()
    {
        SetTime();
        SettingInitHero();
        SettingInitHand();
        isPlayerTurn = true;
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
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            GiveCardToHand(playerDeck, playerHandTransform);
        }
        else
        {
            GiveCardToHand(enemyDeck, enemyHandTransform);
        }
        TurnCalc();
    }
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
        button.image.sprite = playerTurnSprite;
        Debug.Log("味方ターン");
        playerHeroController.model.ResetMP();
        playerHeroController.ReShow();
        SetCanAttackAllFieldUnit(playerFields, true);
        SetCanAttackAllFieldUnit(enemyFields, false);
    }
    IEnumerator EnemyTurn()
    {
        button.image.sprite = enemyTurnSprite;
        Debug.Log("相手ターン");
        enemyHeroController.model.ResetMP();
        enemyHeroController.ReShow();
        SetCanAttackAllFieldUnit(playerFields, false);
        SetCanAttackAllFieldUnit(enemyFields, true);

        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        if (handCardList.Length != 0)
        {
            foreach (var canPutCard in handCardList.Where(i => i.model.cost <= GetEnemyMP()))
            {
                if(canPutCard.model.cost > GetEnemyMP()) { continue; }
                foreach (var enemyField in enemyFields)
                {
                    if (enemyField.childCount == 0)
                    {
                        StartCoroutine(canPutCard.movement.MoveToFiled(enemyField));
                        yield return new WaitForSeconds(0.25f);
                        ReduceMP(canPutCard.model.cost, false);
                        canPutCard.model.isFieldCard = true;
                        yield return new WaitForSeconds(0.75f);
                        break;
                    }
                }
            }

        }

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
                        StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerField));
                        yield return new WaitForSeconds(1f);
                        CardsBattle(canAttackFieldEnemyCard,cc);
                        break;
                    }
                    
                }
            }
            if (canAttackFieldEnemyCard.model.canAttack && playerHeroController.model.isAlive)
            {
                StartCoroutine(canAttackFieldEnemyCard.movement.MoveToTarget(playerHeroController.transform));
                yield return new WaitForSeconds(1f);
                AttackTohero(canAttackFieldEnemyCard, false);
            }
        }

        yield return new WaitForSeconds(1f);

        ChangeTurn();
    }
    public void CardsBattle(CardController attacker, CardController target)
    {
        if (attacker.model.isPlayerCard == target.model.isPlayerCard) { return; }
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
            enemyHeroController.ReShow();
            if (!enemyHeroController.model.isAlive) { 
                Invoke("ViewResultPanel",1f);
            }
        }
        else {
            playerHeroController.Damage(attacker);
            playerHeroController.ReShow();
            if (!playerHeroController.model.isAlive) {
                Invoke("ViewResultPanel", 1f);
            }
        }
    }
    public int GetPlayerMP()
    {
        return playerHeroController.model.mp;
    }
    public void ReduceMP(int reduce, bool isPlayerCard) {
        if (isPlayerCard)
        {
            playerHeroController.ReduceMP(reduce);
        }
        else { enemyHeroController.ReduceMP(reduce); }
    }
    public int GetEnemyMP()
    {
        return enemyHeroController.model.mp;
    }
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
}
