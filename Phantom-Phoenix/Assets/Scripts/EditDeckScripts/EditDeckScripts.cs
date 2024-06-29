using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// デッキ編成画面　主にカード一覧のページ管理を行う　1ページ最大8枚
/// </summary>
public class EditDeckManager : MonoBehaviour
{
    [SerializeField] Button ButtonLeft;
    [SerializeField] Button ButtonRight;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform Stock;
    [SerializeField] Transform Deck;
    List<CardEntity> stockCards;
    List<int> cardIDs;
    private int pages = 1;
    private void Start()
    {
        var heroID = GameDataManager.instance.editDeckHeroID;
        stockCards = GameDataManager.instance.cardlist.cl.Where(c => c.hero == CardEntity.Hero.common || (int)c.hero == heroID).OrderByDescending(c => c.hero).ThenBy(c => c.ID).ToList();
        cardIDs = stockCards.Select(c => c.ID).ToList();

        ButtonLeft.onClick.AddListener(() =>
        {
            AudioManager.instance.SoundCardMove();
            ButtonPageOnClick(true);
        });
        ButtonRight.onClick.AddListener(() =>
        {
            AudioManager.instance.SoundCardMove();
            ButtonPageOnClick(false);
        });

        DeckModel deckmodel = new DeckModel().Init();
        if(deckmodel.useHeroID == GameDataManager.instance.editDeckHeroID)
        {
            deckmodel.deck.OrderBy(i => i).ToList().ForEach(i =>
            {
                Instantiate(cardPrefab, Deck).GetComponent<EditDeckCardController>().Init(i);
            });
        }

        //最初のページを表示
        GetNewStock();

        
    }
    private void GetNewStock()
    {
        int viewCount = 8;
        int id = 0;

        if (cardIDs.Count is int cc && cc < pages * 8)
        {
            if (cc <= (pages - 1) * 8) { pages--; } //表示するカードがないので、page--で対処
            viewCount = cc % 8;
            if(viewCount == 0) { viewCount = 8; } //8で割り切れるなら8枚表示する
        }

        //今、カード一覧に表示されているカードを消す
        foreach (Transform child in Stock) { 
           Destroy(child.gameObject);
        }

        //ページ数を基にしてカードを取得する
        do
        {
            EditDeckCardController card = Instantiate(cardPrefab, Stock).GetComponent<EditDeckCardController>();
            card.Init(cardIDs[(id++) + (8 * pages) - 8]);
        } while (--viewCount > 0);
    }
    private void ButtonPageOnClick(bool isLeft)
    {
        //左ボタンは前ページ
        if (isLeft)
        {
            if (--pages <= 0) { pages = 1; }
        }
        else //右ボタンは次ページ
        {
            ++pages;
        }
        GetNewStock();
    }
}
