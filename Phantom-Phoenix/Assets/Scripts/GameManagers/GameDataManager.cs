using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataManager : MonoBehaviour
{
    public enum Scene
    {
        menu,deck,battle
    }
    private Scene _scene;
    public Scene scene { get; set; }
    [NonSerialized]
    //AI戦かPlayer戦か
    public bool isOnlineBattle;
    //部屋を立てた側か否か
    [NonSerialized]
    public bool isMaster;
    public PhantomPhoenixCardList cardlist;
    public static GameDataManager instance {  get; private set; }
    [NonSerialized]
    public int DeckHeroID = 1;
    private void Awake()
    {
        scene = Scene.menu;

        //初期デッキが必要なら用意 1がないなら2も3も4もない
        if (!PlayerPrefs.HasKey("PlayerDeckData1"))
        {
            MakeTemplateDeck(1, new List<int> { 5, 9, 16, 23, 23, 23, 33, 33, 34, 34, 38, 38, 38, 39, 39, 41, 44, 44, 45, 45, 45, 46, 48, 48, 48, 49, 49, 49, 52, 52 });
            MakeTemplateDeck(2, new List<int> { 53, 53, 53, 54, 54, 55, 55, 56, 56, 56, 57, 57, 57, 62, 63, 63, 63, 64, 67, 67, 67, 68, 69, 69, 69, 71, 71, 72, 72, 72 });
            MakeTemplateDeck(3, new List<int> { 5, 9, 10, 10, 73, 74, 74, 74, 75, 75, 76, 76, 76, 77, 78, 78, 81, 82, 83, 83, 84, 84, 86, 86, 88, 89, 90, 90, 91, 91 });
            MakeTemplateDeck(4, new List<int> { 26, 32, 93, 93, 94, 95, 95, 95, 96, 97, 97, 100, 100, 102, 103, 103, 104, 104, 104, 105, 105, 105, 106, 106, 106, 109, 109, 109, 111, 112 });
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
    private void MakeTemplateDeck(int uHID, List<int> dk)
    {
        DeckData data = new DeckData()
        {
            useHeroID = uHID,
            deck = dk
        };
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString($"PlayerDeckData{uHID}", json);
        PlayerPrefs.Save();
    }
}
