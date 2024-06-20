using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    //AIêÌÇ©PlayerêÌÇ©
    public bool isOnlineBattle;
    //ïîâÆÇóßÇƒÇΩë§Ç©î€Ç©
    public bool isMaster;
    public static GameDataManager instance {  get; private set; }
    private void Awake()
    {
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
