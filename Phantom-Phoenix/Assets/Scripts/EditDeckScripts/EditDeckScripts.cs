using System.Collections;
using System.Collections.Generic;
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
    private int pages = 1;
    private void Start()
    {
        
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

        //最初のページを表示
        GetNewStock();

        DeckModel deckmodel = new DeckModel().Init();
        deckmodel.deck.OrderBy(i => i).ToList().ForEach(i =>
        {
            Instantiate(cardPrefab, Deck).GetComponent<EditDeckCardController>().Init(i);
        });
    }
    private void GetNewStock()
    {
        int viewCount = 8;
        int id = 1;

        if (GameDataManager.instance.cardlist.cl.Count is int clc && clc < pages * 8)
        {
            if (clc < (pages - 1) * 8) { pages--; } //表示するカードがないので、page--で対処
            viewCount = clc % 8;
        }

        //今、カード一覧に表示されているカードを消す
        foreach (Transform child in Stock) { 
           Destroy(child.gameObject);
        }

        //ページ数を基にしてカードを取得する
        do
        {
            EditDeckCardController card = Instantiate(cardPrefab, Stock).GetComponent<EditDeckCardController>();
            card.Init((id++) + (8 * pages) - 8);
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
