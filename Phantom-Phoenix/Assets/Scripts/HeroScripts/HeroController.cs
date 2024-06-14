using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    HeroView view;
    public HeroModel model;
    [SerializeField]
    bool isPlayer;
    private void Start()
    {
        view = GetComponent<HeroView>();
    }

    public void Init(int HeroID)
    {
        model = new HeroModel(HeroID, isPlayer);
        view.Show(model);
    }
    public void Damage(CardController enemyCard)
    {
        model.Damage(enemyCard.model.atk);
        enemyCard.SetCanAttack(false);
    }
    public void ReShow()
    {
        view.ReShow(model);
    }
    public void ReduceMP(int reduce)
    {
        model.ReduceMP(reduce);
        view.ReShow(model);
    }
    public bool GetIsPlayer()
    {
        return model.isPlayer;
    }
}
