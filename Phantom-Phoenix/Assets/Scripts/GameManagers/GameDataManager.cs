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

        //初期デッキが必要なら用意 1がないなら2も3も4もないよ
        if (!PlayerPrefs.HasKey("PlayerDeckData1"))
        {
            //エルフの初期デッキ
            MakeTemplateDeck(1, new List<int> { 5, 33, 33, 9, 34, 34, 38, 38, 38, 39, 39, 41, 16, 44, 44, 45, 45, 45, 46, 48, 48, 48, 23, 23, 23, 49, 49, 49, 52, 52 });
            //ウィッチの初期デッキ
            MakeTemplateDeck(2, new List<int> { 53, 53, 53, 54, 54, 55, 55, 56, 56, 56, 57, 57, 57, 62, 63, 63, 63, 64, 67, 67, 67, 68, 69, 69, 69, 71, 71, 72, 72, 72 });
            //キングの初期デッキ
            MakeTemplateDeck(3, new List<int> { 5, 73, 74, 74, 74, 9, 75, 75, 76, 76, 76, 77, 78, 78, 10, 10, 81, 82, 83, 83, 84, 84, 86, 86, 88, 89, 90, 91, 91, 91 });
            //デーモンの初期デッキ
            MakeTemplateDeck(4, new List<int> { 93, 93, 94, 95, 95, 95, 96, 97, 97, 100, 100, 102, 103, 103, 104, 104, 104, 105, 105, 105, 106, 106, 106, 109, 109, 109, 26, 111, 112, 32 });
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
        DeckData data = new DeckData(){useHeroID = uHID, deck = dk};
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString($"PlayerDeckData{uHID}", json);
        PlayerPrefs.Save();
    }
}
