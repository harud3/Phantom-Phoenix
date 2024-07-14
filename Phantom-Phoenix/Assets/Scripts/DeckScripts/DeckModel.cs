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
        //json�t�@�C������L�^�����f�b�L���擾����@SelectHeroScene�őI�������q�[���[�̃f�b�L��ΏۂƂ���
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
            Debug.Log("�f�b�L�f�[�^�����݂��܂���");
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
