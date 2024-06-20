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
#if UNITY_EDITOR
            deck = data.deck.ToList();
            string str = "";
            deck.ForEach(i => str += $"{i.ToString()},");
            Debug.Log(str);
#endif
        }
        else
        {
            Debug.Log("PlayerDeckData‚ª‘¶Ý‚µ‚Ü‚¹‚ñ");
        }

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
