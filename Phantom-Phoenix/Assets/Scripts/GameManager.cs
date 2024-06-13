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
        Debug.Log("�����^�[��");
        SetCanAttackAllFieldUnit(playerFields, true);
        SetCanAttackAllFieldUnit(enemyFields, false);
    }
    void EnemyTurn()
    {
        button.image.sprite = enemyTurnSprite;
        Debug.Log("����^�[��");
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
        Debug.Log($"{(isPlayerTurn? "�����^�[��" :"����^�[��")} {attacker.model.name}����{target.model.name}�ɍU��");
        attacker.Attack(target);
        target.Attack(attacker);
        Debug.Log($"{target.model.name}��{attacker.model.atk}�_���[�W {target.model.name}�̎c��HP{target.model.hp}");
        Debug.Log($"{attacker.model.name}��{target.model.atk}�_���[�W {attacker.model.name}�̎c��HP{attacker.model.hp}");
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
