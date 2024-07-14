using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckModel
{
    [NonSerialized]
    public int useHeroID;

    [NonSerialized]
    public List<int> deck;
    public DeckModel Init()
    {
        //jsonファイルから記録したデッキを取得する　SelectHeroSceneで選択したヒーローのデッキを対象とする
        string DeckHeroID = GameDataManager.instance.DeckHeroID.ToString();
        if ( PlayerPrefs.HasKey($"PlayerDeckData{DeckHeroID}"))
        {
            string json = PlayerPrefs.GetString($"PlayerDeckData{DeckHeroID}");
            DeckData data = JsonUtility.FromJson<DeckData>(json);
            useHeroID = data.useHeroID;
            deck = data.deck.OrderBy(i => Guid.NewGuid()).ToList();
        }
        else
        {
            Debug.Log("デッキデータが存在しません");
        }

        return this;
    }
    public DeckModel Init(int useheroID,int[] deckIDs)
    {
        this.useHeroID = useheroID;
        deck = deckIDs.ToList();
        return this;
    }
}
