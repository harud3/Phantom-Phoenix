using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// デッキ編成画面　主にカード一覧のページ管理を行う　1ページ最大8枚
/// </summary>
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
            Instantiate(cardPrefab, Deck).GetComponent<DeckSceneCardController>().Init(i);
        });
    }
    private void GetNewStock()
    {
        
        id = 1;
        //これがない場合、ページを-1して辻褄合わせ
        if (Resources.Load<CardEntity>($"CardEntityList/Card{id + (8 * pages) - 8}") == null) { pages--; }

        //今、カード一覧に表示されているカードを消す
        foreach (Transform child in Stock) { 
           GameObject.Destroy(child.gameObject);
        }

        //ページ数を基にしてカードを取得する
        int viewCount = 0;
        do
        {
            if (Resources.Load<CardEntity>($"CardEntityList/Card{id + (8 * pages) - 8}") == null) { break; } //取得できなかったら抜ける
            DeckSceneCardController card = Instantiate(cardPrefab, Stock).GetComponent<DeckSceneCardController>();
            card.Init((id++) + (8 * pages) - 8);
        } while (++viewCount < 8);
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
