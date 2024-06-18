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
    [SerializeField] Image backFrameImage;
    [SerializeField] Image backCost;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Image backATK;
    [SerializeField] TextMeshProUGUI atkText;
    [SerializeField] Image backHP;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI cardText;
    [SerializeField] GameObject selectablePanel;
    [SerializeField] GameObject frameTaunt;
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void SetCard(CardModel cardModel)
    {
        iconImage.sprite = cardModel.icon;
        frameImage.sprite = Resources.Load<Sprite>($"Frames/{cardModel.category}{cardModel.rarity}");
        if (!cardModel.isPlayerCard)
        {
            Show(false);
        }
        if(cardModel.category == CardEntity.Category.spell)
        {
            backATK.gameObject.SetActive(false);
            backHP.gameObject.SetActive(false);
        }
        atkText.text = cardModel.atk.ToString();
        hpText.text = cardModel.hp.ToString();
        costText.text = cardModel.cost.ToString();
        cardText.text = cardModel.cardText.Replace(@"\n","\n");
    }
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void HideCost(bool isActive)
    {
        costText.transform.parent.gameObject.SetActive(isActive);
    }
    public void Show(bool viewOpenSide)
    {
        backFrameImage.gameObject.SetActive(!viewOpenSide);
        frameImage.gameObject.SetActive(viewOpenSide);
        backCost.gameObject.SetActive(viewOpenSide);
        backATK.gameObject.SetActive(viewOpenSide);
        backHP.gameObject.SetActive(viewOpenSide);
    }
    /// <summary>
    /// ��{�I��Controller��ʂ��ČĂԂ��ƂɂȂ�
    /// </summary>
    /// <param name="cardModel"></param>
    public void ReShow(CardModel cardModel)
    {
        atkText.text = cardModel.atk.ToString();
        hpText.text = cardModel.hp.ToString();
    }
    /// <summary>
    /// �J�[�h�𓮂����邩�ۂ��̉���
    /// </summary>
    /// <param name="isActive"></param>
    public void SetActiveSelectablePanel(bool isActive)
    {
        selectablePanel.SetActive(isActive);
    }
    public void SetViewFrameTaunt(bool isView)
    {
        frameTaunt.SetActive(isView);
    }
}
