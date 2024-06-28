using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHeroController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int heroID; //各種ヒーロー画像ごとに事前設定
    [SerializeField] private GameObject selectPanel; //各種ヒーロー画像ごとに事前設定
    private void Start()
    {
        if(GameDataManager.instance.editDeckHeroID == heroID)
        {
            setSelectPanel(true);
        }
        else
        {
            setSelectPanel(false);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        GameDataManager.instance.editDeckHeroID = heroID;
    }
    private void setSelectPanel(bool isSelect)
    {
        selectPanel.SetActive(isSelect);
    }
}
