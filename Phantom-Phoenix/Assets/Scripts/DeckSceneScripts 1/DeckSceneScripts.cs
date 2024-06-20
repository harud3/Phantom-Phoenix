using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeckSceneManager : MonoBehaviour
{
    [SerializeField] Button ButtonLeft;
    [SerializeField] Button ButtonRight;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform Stock;
    [SerializeField] Transform Deck;
    private int pages = 1;
    private int id = 1;
    private void Start()
    {
        ButtonLeft.onClick.AddListener(() => ButtonPageOnClick(true));
        ButtonRight.onClick.AddListener(() => ButtonPageOnClick(false));

        GetNewStock();

        DeckModel deckmodel = new DeckModel().Init();
        deckmodel.deck.OrderBy(i => i).ToList().ForEach(i =>
        {
            Instantiate(cardPrefab, Deck).GetComponent<DeckSceneCardController>().Init(i);
        });
    }
    private void GetNewStock()
    {
        id = 1;
        if (Resources.Load<CardEntity>($"CardEntityList/Card{id + (8 * pages) - 8}") == null) { pages--; }

        foreach (Transform child in Stock) { 
           GameObject.Destroy(child.gameObject);
        }
        int viewCount = 0;
        do
        {
            if (Resources.Load<CardEntity>($"CardEntityList/Card{id + (8 * pages) - 8}") == null) { break; }
            DeckSceneCardController card = Instantiate(cardPrefab, Stock).GetComponent<DeckSceneCardController>();
            card.Init((id++) + (8 * pages) - 8);
        } while (++viewCount < 8);
    }
    private void ButtonPageOnClick(bool isLeft)
    {
        if (isLeft)
        {
            if (--pages <= 0) { pages = 1; }
        }
        else
        {
            ++pages;
        }
        GetNewStock();
    }
}
