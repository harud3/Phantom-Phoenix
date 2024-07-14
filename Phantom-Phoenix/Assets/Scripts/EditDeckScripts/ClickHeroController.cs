using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardEntity;

public class ClickHeroController : MonoBehaviour
{
    [SerializeField] private List<Button> heros = new List<Button>(4); //�e��q�[���[�{�^�����ƂɎ��O�ݒ�
    [SerializeField] private List<GameObject> selectPanel = new List<GameObject>(4); //�e��q�[���[�摜���ƂɎ��O�ݒ�
    private void Start()
    {
        setSelectPanel(GameDataManager.instance.DeckHeroID);

        var heroID = 1;
        foreach ((Button ButtonHero, int heroID) hero in heros.Select((h, i) => (h, heroID++)))
        {
            //�q�[���[�{�^������������A�g�p����q�[���[��ς��ĕێ�����
            hero.ButtonHero.onClick.AddListener(() =>
            {
                if(GameDataManager.instance.DeckHeroID != hero.heroID)
                {
                    AudioManager.instance.SoundButtonClick3();
                    GameDataManager.instance.DeckHeroID = hero.heroID;
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
