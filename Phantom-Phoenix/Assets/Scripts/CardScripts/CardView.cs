using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �J�[�h�̌�����
/// </summary>
public class CardView : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Image frameImage;
    [SerializeField] Image costIcon;
    [SerializeField] Image atkIcon;
    [SerializeField] Image hpIcon;
    [SerializeField] TextMeshProUGUI cardText;
    [SerializeField] GameObject selectablePanel;
    [SerializeField] GameObject frame����;
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void Show(CardModel cardModel)
    {
        iconImage.sprite = cardModel.icon;
        frameImage.sprite = Resources.Load<Sprite>($"Frames/{cardModel.categoryRarity}");
        atkIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.atk}");
        hpIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.hp}");
        costIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.cost}");
        cardText.text = cardModel.cardText;
    }
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void HideCost(bool isActive)
    {
        costIcon.transform.parent.parent.gameObject.SetActive(isActive);
    }
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void ReShow(CardModel cardModel)
    {
        hpIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.hp}");
        atkIcon.sprite = Resources.Load<Sprite>($"Numbers/s{cardModel.atk}");
    }
    /// <summary>
    /// �J�[�h�𓮂����邩�ۂ��̉���
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        selectablePanel.SetActive(isActive);
    }
    public void SetViewFrame����(bool isView)
    {
        frame����.SetActive(isView);
    }
}
