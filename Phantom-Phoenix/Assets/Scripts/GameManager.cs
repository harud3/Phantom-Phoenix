using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    private Transform[] playerFields, enemyFields;
    [NonSerialized]
    public static GameManager instance;
    public bool isPlayerTurn { get { return _isPlayerTurn; } private set { _isPlayerTurn = value; } }
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
    void StartGame()
    {
        SettingInitHand();
        isPlayerTurn = true;
        TurnCalc();
    }
    void SettingInitHand()
    {
        for (int i = 0; i < 3; i++)
        {
            CreateCard(playerHandTransform);
            CreateCard(enemyHandTransform);
        }
    }

    void TurnCalc()
    {
        if (isPlayerTurn) {
            PlayerTurn();
        }
        else {
            EnemyTurn();
        }
    }
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            CreateCard(playerHandTransform);
        }
        else
        {
            CreateCard(enemyHandTransform);
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
        SetCanAttackAllFieldUnit(playerFields, true);
        SetCanAttackAllFieldUnit(enemyFields, false);
    }
    void EnemyTurn()
    {
        button.image.sprite = enemyTurnSprite;
        Debug.Log("相手ターン");
        SetCanAttackAllFieldUnit(playerFields, false);
        SetCanAttackAllFieldUnit(enemyFields, true);

        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        CardController enemyCard = handCardList[0];
        foreach(var enemyField in enemyFields)
        {
            if(enemyField.childCount == 0)
            {
                enemyCard.movement.SetcardTransform(enemyField);
                break;
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
                        CardsBattle(canAttackFieldEnemyCard,cc);
                        break;
                    }
                    
                }
            }
        }
        

        ChangeTurn();
    }
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
    void CreateCard(Transform hand)
    {
        if (hand.childCount == 10) { return; }
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(1);
    }
}
