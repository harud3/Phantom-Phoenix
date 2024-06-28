using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    [NonSerialized]
    //AI�킩Player�킩
    public bool isOnlineBattle;
    //�����𗧂Ă������ۂ�
    [NonSerialized]
    public bool isMaster;
    public PhantomPhoenixCardList cardlist;
    public static GameDataManager instance {  get; private set; }
    [NonSerialized]
    public int editDeckHeroID = 1;
    private void Awake()
    {

        //�����f�b�L���K�v�Ȃ�p��
        if (!PlayerPrefs.HasKey("PlayerDeckData"))
        {
            DeckData data = new DeckData()
            {
                useHeroID = 1,
                deck = new List<int> { 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 5, 6, 6, 6, 7, 7, 7, 8, 9, 9, 9, 10, 10, 10, 11, 11, 11, 12, 13, 13 }
            };
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString("PlayerDeckData", json);
            PlayerPrefs.Save();
        }
        else
        {
            DeckModel deckmodel = new DeckModel().Init();
            editDeckHeroID = deckmodel.useHeroID;
        }
            if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
