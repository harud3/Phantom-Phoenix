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
    public DeckModel Init(DeckEntity deckEntity)
    {
        useHeroID = deckEntity.useHeroID;
        deck = deckEntity.deck.OrderBy(i => Guid.NewGuid()).ToList();
#if UNITY_EDITOR
        deck = deckEntity.deck.ToList();
        string str = "";
        deck.ForEach(i => str += $"{i.ToString()},");
        Debug.Log(str);
#endif

        return this;
    }
    public DeckModel Init(int useheroID,int[] deckIDs)
    {
        this.useHeroID = useheroID;
        deck = deckIDs.ToList();
#if UNITY_EDITOR
        deck = deckIDs.ToList();
        string str = "";
        deck.ForEach(i => str += $"{i.ToString()},");
        Debug.Log(str);
#endif

        return this;
    }
}
