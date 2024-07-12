using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardEntity;

public class ClickHeroController : MonoBehaviour
{
    [SerializeField] private List<Button> heros = new List<Button>(4); //各種ヒーローボタンごとに事前設定
    [SerializeField] private List<GameObject> selectPanel = new List<GameObject>(4); //各種ヒーロー画像ごとに事前設定
    private void Start()
    {
        setSelectPanel(GameDataManager.instance.editDeckHeroID);

        var heroID = 1;
        foreach ((Button ButtonHero, int heroID) hero in heros.Select((h, i) => (h, heroID++)))
        {
            hero.ButtonHero.onClick.AddListener(() =>
            {
                if(GameDataManager.instance.editDeckHeroID != hero.heroID)
                {
                    AudioManager.instance.SoundButtonClick3();
                    GameDataManager.instance.editDeckHeroID = hero.heroID;
                    setSelectPanel(hero.heroID);
                }
                
            });
        }
    }
    private void setSelectPanel(int heroID)
    {
        selectPanel.ForEach(i => { i.SetActive(false); });
        selectPanel[heroID - 1].SetActive(true);
    }
}
