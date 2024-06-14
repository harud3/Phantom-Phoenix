using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CardController : MonoBehaviour
{
    CardView view;
    public CardModel model;
    [NonSerialized]
    public CardMovement movement;
    private void Awake()
    {
        view = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
    }

    public void Init(int CardID, bool isPlayer)
    {
        model = new CardModel(CardID, isPlayer);
        view.Show(model);
    }
    public void SetCanAttack(bool canAttack)
    {
        model.canAttack = canAttack;
        view.SetActiveSelectablePanel(canAttack);
    }
    public void Attack(CardController enemyCard)
    {
        model.Attack(enemyCard);
        SetCanAttack(false);
    }
    public void CheckAlive()
    {
        if (model.isAlive) {
            view.ReShow(model);
        }
        else
        {
            Destroy(this.gameObject);
            
        }
    }
}
