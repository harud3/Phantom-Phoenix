using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHeroController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private int heroID; //�e��q�[���[�摜���ƂɎ��O�ݒ�
    [SerializeField] private GameObject selectPanel; //�e��q�[���[�摜���ƂɎ��O�ݒ�
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
