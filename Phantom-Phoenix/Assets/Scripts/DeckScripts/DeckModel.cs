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
        if (PlayerPrefs.HasKey("PlayerDeckData"))
        {
            string json = PlayerPrefs.GetString("PlayerDeckData");
            DeckData data = JsonUtility.FromJson<DeckData>(json);

            useHeroID = data.useHeroID;
            deck = data.deck.OrderBy(i => Guid.NewGuid()).ToList();
        }
        else
        {
            Debug.Log("PlayerDeckDataÇ™ë∂ç›ÇµÇ‹ÇπÇÒ");
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
